﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Point = flash.geom.Point;
using Rectangle = flash.geom.Rectangle;

using flash;
using Array = flash.Array;
using Math = System.Math;
using XorRandom = com.robotacid.util.XorRandom;

namespace com.robotacid.engine
{
    /// <summary>
    /// Manipulates templates generated by the Room class to conduct a game
    ///
	/// The properties of elements can be examined to deduce what animations took place
	/// (see Level)
	///
	/// logic at this tier should remain architecture independant for portability
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class LevelData {

        public Room room;
		public Boolean active;
		public Point player;
		public int playerDir;
		public int oldPlayerX;
		public int oldPlayerY;
		public int width;
		public int height;
		public Array<Array<int>> map;/*Array*/
		public Array<Array<int>> copyBuffer;/*Array*/
		public Action<int, int, int, int, int> killCallback;
		public Action<int, int, int> pushCallback;
		public Action<int, int, int> swapCallback;
		public Action<int, int> generateCallback;
		public Action<int, int, int, int> displaceCallback;
		public Action<int, int, int> endingCallback;
		public Action<int, int, int> blockedCallback;
		public Point playerPush;
		public Array<Point> allies;/*Point*/
		public Array<Point> enemies;/*Point*/
		public Point doorOpened;
		public int food;
		public int turns;
		public Boolean ended;

        public static Array<Array<int>> pathMap;
		public static Boolean initialised;

        public const int ENEMY_ACTIVE_RADIUS = 5;
		public const int PATH_MAP_SIZE = 1 + ENEMY_ACTIVE_RADIUS * 2;
		public const int PATH_WALL = int.MaxValue;
		public const int FOOD_MAX = 48;
		public const int FOOD_KILL = 8;
		
		// temp
		private Point p;

        public LevelData(Room room, int width, int height) {
			this.room = room;
			player = new Point(room.startX, room.startY);
			this.width = width;
			this.height = height;
			map = Room.create2DArray(width, height, Room.VOID);
			copyBuffer = Room.create2DArray(room.width, room.height, Room.VOID);
			room.copyTo(map, 0, 0);
			map[player.y][player.x] = Room.PLAYER | Room.ALLY;
			food = FOOD_MAX;
			active = true;
			allies = new Array<Point>();
			enemies = new Array<Point>();
			turns = 0;
		}

        public static void init() {
			initialised = true;
			//Room.init(); //PWC - made this static constructor
			pathMap = Room.create2DArray(1 + ENEMY_ACTIVE_RADIUS * 2, 1 + ENEMY_ACTIVE_RADIUS * 2, PATH_WALL);
		}

        public void playerTurn(int dir) {
			playerDir = dir;
			// flush player status first, then clear the map for the enemies after the player has moved
			map[player.y][player.x] &= ~(Room.UP_DOWN_LEFT_RIGHT | Room.ATTACK | Room.BLOCKED | Room.PUSHED);
			oldPlayerX = player.x;
			oldPlayerY = player.y;
			player = move(player.x, player.y, playerDir, Room.ALLY);
			// a new room may have been created, or the player may have been pushed
			if(playerPush != null){
				player.x += playerPush.x;
				player.y += playerPush.y;
				oldPlayerX += playerPush.x;
				oldPlayerY += playerPush.y;
				playerPush = null;
			}
			int playerProperty = map[player.y][player.x];
			int swapProperty = map[oldPlayerY][oldPlayerX];
			flushStatus(player.x, player.y, ENEMY_ACTIVE_RADIUS + 2);
			// reload player behaviour and swapped position behaviour
			map[player.y][player.x] = playerProperty;
			if((swapProperty & Room.SWAP) > 0) map[oldPlayerY][oldPlayerX] = swapProperty & ~(Room.ATTACK | Room.BLOCKED);
			if(!((map[player.y][player.x] & Room.BLOCKED) > 0)){
				turns++;
				food--;
				allies.length = enemies.length = 0;
				// entities need to act based on their distance to the player - hence we used the pathMap
				fillPathMap(player.x, player.y, null, Room.ALLY, allies);
				moveEntities(allies);
			}
		}

        public void enemyTurn() {
			enemies.length = allies.length = 0;
			// entities need to act based on their distance to the player - hence we used the pathMap
			fillPathMap(player.x, player.y, null, Room.ALLY, allies);
			allies.push(player); // path map ignores the starting square
			fillPathMap(player.x, player.y, allies.slice(), Room.ENEMY, enemies, Room.DOOR);
			moveEntities(enemies);
			// the player turns to stone (swap block) when out of food
			if(((map[player.y][player.x] & Room.PLAYER) == Room.PLAYER) && food == 0) map[player.y][player.x] = Room.SWAP | Room.WALL;
		}
		
		/* Call to execute an entire turn without animation */
		public void fullTurn(int dir) {
			playerTurn(dir);
			if(!((map[player.y][player.x] & Room.BLOCKED) > 0)) {
				enemyTurn();
			}
		}

        /* Player exists and is not surrounded by walls */
		public Boolean alive() {
			return ended || (
				((map[player.y][player.x] & Room.PLAYER) > 0) && !(
					(player.y == 0 || (((map[player.y - 1][player.x] & (Room.WALL | Room.VOID)) > 0) && !((map[player.y - 1][player.x] & Room.SWAP) > 0))) &&
					(player.x == width - 1 || (((map[player.y][player.x + 1] & (Room.WALL | Room.VOID)) > 0) && !((map[player.y][player.x + 1] & Room.SWAP) > 0))) &&
					(player.y == height - 1 || (((map[player.y + 1][player.x] & (Room.WALL | Room.VOID)) > 0) && !((map[player.y + 1][player.x] & Room.SWAP) > 0))) &&
					(player.x == 0 || (((map[player.y][player.x - 1] & (Room.WALL | Room.VOID)) > 0) && !((map[player.y][player.x - 1] & Room.SWAP) > 0)))
				) &&
				food > 0
			);
		}

        /* Check if moving into a given square means being attacked - assumes the square is empty */
		public Boolean getCheck(int x, int y) {
			return (
				(y > 0 && ((map[y - 1][x] & Room.ENEMY) > 0) && (
					(
						((map[y - 1][x] & Room.TURNER) > 0) && ((map[y - 1][x] & Room.DOWN) > 0)
					) || (
						((map[y - 1][x] & (Room.TRAP | Room.MOVER)) > 0) && ((map[y - 1][x] & Room.M_DOWN) > 0)
					)
				) && !((map[y - 1][x] & (Room.GENERATOR | Room.VIRUS)) > 0) ||
				(x < width - 1 && ((map[y][x + 1] & Room.ENEMY) > 0) && (
					(
						((map[y][x + 1] & Room.TURNER) > 0) && ((map[y][x + 1] & Room.LEFT) > 0)
					) || (
						((map[y][x + 1] & (Room.TRAP | Room.MOVER)) > 0) && ((map[y][x + 1] & Room.M_LEFT) > 0))
					)
				) && !((map[y][x + 1] & (Room.GENERATOR | Room.VIRUS)) > 0)) ||
				(y < height - 1 && ((map[y + 1][x] & Room.ENEMY) > 0) && (
					(
						((map[y + 1][x] & Room.TURNER) > 0) && ((map[y + 1][x] & Room.UP) > 0)
					) || (
						((map[y + 1][x] & (Room.TRAP | Room.MOVER)) > 0) && ((map[y + 1][x] & Room.M_UP) > 0)
					)
				) && !((map[y + 1][x] & (Room.GENERATOR | Room.VIRUS)) > 0)) ||
				(x > 0 && ((map[y][x - 1] & Room.ENEMY) > 0) && (
					(
						((map[y][x - 1] & Room.TURNER) > 0) && ((map[y][x - 1] & Room.RIGHT) > 0)
					) || (
						((map[y][x - 1] & (Room.TRAP | Room.MOVER)) > 0)) && ((map[y][x - 1] & Room.M_RIGHT) > 0)
					)
				) && !((map[y][x - 1] & (Room.GENERATOR | Room.VIRUS)) > 0)
			);
		}

        /* Check if a player movement would be blocked */
		public Boolean blockedDir(int dir){
			return (
				(dir == Room.UP && (player.y == 0 || (((map[player.y - 1][player.x] & (Room.WALL | Room.VOID)) > 0) && !((map[player.y - 1][player.x] & Room.SWAP) > 0)))) ||
				(dir == Room.RIGHT && (player.x == width - 1 || (((map[player.y][player.x + 1] & (Room.WALL | Room.VOID)) > 0) && !((map[player.y][player.x + 1] & Room.SWAP) > 0)))) ||
				(dir == Room.DOWN && (player.y == height - 1 || (((map[player.y + 1][player.x] & (Room.WALL | Room.VOID)) > 0) && !((map[player.y + 1][player.x] & Room.SWAP) > 0)))) ||
				(dir == Room.LEFT && (player.x == 0 || (((map[player.y][player.x - 1] & (Room.WALL | Room.VOID)) > 0)) && !((map[player.y][player.x - 1] & Room.SWAP) > 0)))
			);
		}
		
		public LevelData copy() {
			var level = new LevelData(room, width, height);
			level.copyData(this);
			return level;
		}

        /* Deep copy */
		public void copyData(LevelData source) {
			int r, c;
			for(r = 0; r < height; r++){
				for(c = 0; c < width; c++){
					map[r][c] = source.map[r][c];
				}
			}
			playerDir = source.playerDir;
			player.x = source.player.x;
			player.y = source.player.y;
		}

        public Library.LevelData saveData(Boolean saveFood = false) {
  
            var obj = saveObject(width, height);
            obj.player.x = player.x;
            obj.player.y = player.y;
            if(saveFood) obj.food = food;
            if(room.type == Room.ADVENTURE){
                obj.endingDist = room.endingDist;
                obj.turns = turns;
            }
            int r, c;
            for(r = 0; r < height; r++){
                for(c = 0; c < width; c++){
                    obj.map[r][c] = map[r][c];
                }
            }
            return obj;
        }

        ///* Creates a template for saving data */
        public static Library.LevelData saveObject(int width, int height) {
            var obj = new Library.LevelData();
            obj.map = new Array<Array<int>>();
            obj.playerDir = Room.UP;
            obj.player = new Library.Point();
            obj.player.x = (int)(width * 0.5) >> 0; 
            obj.player.y = (int)(height * 0.5) >> 0;
            
            //TODO: map is empty!
            int r, c;
            for(r = 0; r < height; r++){
                obj.map.push(new Array<int>());
                for(c = 0; c < width; c++){
                    obj.map[r].push(Room.VOID);
                }
            }
            return obj;
        }

        public static void writeToObject(Library.LevelData source, Library.LevelData target, int width, int height) {
            target.player.x = source.player.x;
            target.player.y = source.player.y;
            int r, c;
            for(r = 0; r < height; r++){
                for(c = 0; c < width; c++){
                    target.map[r][c] = source.map[r][c];
                }
            }
        }

        public void loadData(Library.LevelData obj) {
            food = FOOD_MAX;
            if(obj != null){
                int r, c;
                for(r = 0; r < height; r++){
                    for(c = 0; c < width; c++){
                        map[r][c] = obj.map[r][c];
                    }
                }
                playerDir = obj.playerDir;
                player.x = obj.player.x;
                player.y = obj.player.y;
                if(obj.food  > 0) food = obj.food;
                if(room.type == Room.ADVENTURE){
                    room.endingDist = obj.endingDist;
                    turns = obj.turns;
                }
            } else {
                room.clear();
                room.copyTo(map, 0, 0);
                player.x = room.startX;
                player.y = room.startY;
            }
            map[player.y][player.x] = Room.PLAYER | Room.ALLY;
        }

        /* Is a given move empty? */
		public Boolean empty(int x, int y, int dir) {
			if(dir == Room.UP){
				return y > 0 && map[y - 1][x] == Room.EMPTY;
			} else if(dir == Room.RIGHT){
				return x < width - 1 && map[y][x + 1] == Room.EMPTY;
			} else if(dir == Room.DOWN){
				return y < height - 1 && map[y + 1][x] == Room.EMPTY;
			} else if(dir == Room.LEFT){
				return x > 0 && map[y][x - 1] == Room.EMPTY;
			}
			return false;
		}

        public void swap(int sx, int sy, int tx, int ty) {
			var source = map[sy][sx];
			var target = map[ty][tx];
			source &= ~(Room.UP_DOWN_LEFT_RIGHT | Room.BLOCKED);
			target &= ~(Room.UP_DOWN_LEFT_RIGHT | Room.BLOCKED);
			if(tx > sx){
				source |= Room.RIGHT;
				target |= Room.LEFT;
			} else if(tx < sx){
				source |= Room.LEFT;
				target |= Room.RIGHT;
			} else if(ty > sy){
				source |= Room.DOWN;
				target |= Room.UP;
			} else if(ty < sy){
				source |= Room.UP;
				target |= Room.DOWN;
			}
			map[ty][tx] = source;
			map[sy][sx] = target;
			if(swapCallback != null) swapCallback(tx, ty, target);
		}

        /* Recursive destruction - BOMB properties trigger this method when killed */
		public void explode(int x, int y, int dir, int explosion = 1) {
			int oppositeDir = Room.oppositeDirection(dir);
			if(y > 0 && !((map[y - 1][x] & (Room.INDESTRUCTIBLE | Room.VOID | Room.DOOR)) > 0) && !((oppositeDir & Room.UP) > 0)){
				kill(x, y - 1, Room.UP, explosion + 1);
			}
			if(x < width - 1 && !((map[y][x + 1] & (Room.INDESTRUCTIBLE | Room.VOID | Room.DOOR)) > 0) && !((oppositeDir & Room.RIGHT) > 0)){
				kill(x + 1, y, Room.RIGHT, explosion + 1);
			}
			if(y < height - 1 && !((map[y + 1][x] & (Room.INDESTRUCTIBLE | Room.VOID | Room.DOOR)) > 0) && !((oppositeDir & Room.DOWN) > 0)){
				kill(x, y + 1, Room.DOWN, explosion + 1);
			}
			if(x > 0 && !((map[y][x - 1] & (Room.INDESTRUCTIBLE | Room.VOID | Room.DOOR)) > 0) && !((oppositeDir & Room.LEFT) > 0)){
				kill(x - 1, y, Room.LEFT, explosion + 1);
			}
		}

        /* Move an element one tile in a direction on the map - blocked movement is recorded */
		public Point move(int x, int y, int dir, int friendly) {
			// store without a direction and set direction
			var property = map[y][x];
			if(((property & Room.TURNER) == Room.TURNER) && !((property & Room.GENERATOR) > 0) && ((property & Room.UP_DOWN_LEFT_RIGHT) > 0)){
				property &= ~(Room.M_UP_DOWN_LEFT_RIGHT);
				if((property & Room.UP) == Room.UP) property |= Room.M_UP;
				else if((property & Room.RIGHT) == Room.RIGHT) property |= Room.M_RIGHT;
				else if((property & Room.DOWN) == Room.DOWN) property |= Room.M_DOWN;
				else if((property & Room.LEFT) == Room.LEFT) property |= Room.M_LEFT;
				property &= ~(Room.UP_DOWN_LEFT_RIGHT);
			}
			property |= dir;
			map[y][x] = property;
			int target;
			map[y][x] |= Room.BLOCKED;
			// if the way is clear, load the space with property unblocked or mark as attacking
			if(dir == Room.UP){
				if(y > 0){
					target = map[y - 1][x];
					if((property & Room.GENERATOR) > 0){
						if(target == Room.EMPTY && ((property & Room.TIMER_0) > 0)){
							map[y - 1][x] = (property & ~(Room.GENERATOR | Room.UP_DOWN_LEFT_RIGHT)) | Room.UP | (((property & Room.TURNER) > 0) ? Room.M_UP : 0);
							if((property & Room.VIRUS) > 0) map[y - 1][x] |= Room.GENERATOR;
							map[y][x] = (property & ~(Room.UP_DOWN_LEFT_RIGHT)) | Room.UP | Room.ATTACK;
							if(generateCallback != null) generateCallback(x, y);
						} else {
							map[y][x] = property;
						}
					} else if(((property & Room.TURNER) > 0) && !((property & Room.M_UP) > 0)){
						map[y][x] = property;
					} else if(((property & Room.PLAYER) > 0) && ((target & Room.SWAP) > 0)){
						swap(x, y, x, y - 1);
						y--;
					} else if((target & Room.WALL) > 0){
						// blocked
					} else if(target == Room.EMPTY){
						map[y - 1][x] = property;
						map[y][x] = Room.EMPTY;
						y--;
					} else if(((target & (Room.ENEMY | Room.ALLY)) > 0) && !((target & friendly) > 0)){
						if((property & Room.WALL) > 0){
							map[y][x] = property;
							push(x, y, Room.UP);
							y--;
						} else {
							map[y][x] = (property | Room.ATTACK) + (((target & Room.PLAYER) > 0) ? Room.KILLER : 0);
							kill(x, y - 1, Room.UP);
						}
					}
					
				}
			} else if(dir == Room.RIGHT){
				if(x < width - 1){
					target = map[y][x + 1];
					if((property & Room.GENERATOR) > 0){
						if(target == Room.EMPTY && ((property & Room.TIMER_0) > 0)){
							map[y][x + 1] = (property & ~(Room.GENERATOR | Room.UP_DOWN_LEFT_RIGHT)) | Room.RIGHT | (((property & Room.TURNER) > 0) ? Room.M_RIGHT : 0);
							if((property & Room.VIRUS) == Room.VIRUS) map[y][x + 1] |= Room.GENERATOR;
							map[y][x] = (property & ~(Room.UP_DOWN_LEFT_RIGHT)) | Room.RIGHT | Room.ATTACK;
							if(generateCallback != null) generateCallback(x, y);
						} else {
							map[y][x] = property;
						}
					} else if(((property & Room.TURNER) > 0) && !((property & Room.M_RIGHT) > 0)){
						map[y][x] = property;
					} else if(((property & Room.PLAYER) > 0) && ((target & Room.SWAP) > 0)){
						swap(x, y, x + 1, y);
						x++;
					} else if((target & Room.WALL) > 0){
						// blocked
					} else if(target == Room.EMPTY){
						map[y][x + 1] = property;
						map[y][x] = Room.EMPTY;
						x++;
                    } else if(((target & (Room.ENEMY | Room.ALLY)) > 0) && !((target & friendly) > 0)){
						if((property & Room.WALL) == Room.WALL){
							map[y][x] = property;
							push(x, y, Room.RIGHT);
							x++;
						} else {
							map[y][x] = (property | Room.ATTACK) + (((target & Room.PLAYER) > 0) ? Room.KILLER : 0);
							kill(x + 1, y, Room.RIGHT);
						}
					}
				}
			} else if(dir == Room.DOWN){
				if(y < height - 1){
					target = map[y + 1][x];
					if((property & Room.GENERATOR) == Room.GENERATOR){
						if(target == Room.EMPTY && ((property & Room.TIMER_0) == Room.TIMER_0)){
							map[y + 1][x] = (property & ~(Room.GENERATOR | Room.UP_DOWN_LEFT_RIGHT)) | Room.DOWN | (((property & Room.TURNER) > 0) ? Room.M_DOWN : 0);
							if((property & Room.VIRUS) > 0) map[y + 1][x] |= Room.GENERATOR;
							map[y][x] = (property & ~(Room.UP_DOWN_LEFT_RIGHT)) | Room.DOWN | Room.ATTACK;
							if(generateCallback != null) generateCallback(x, y);
						} else {
							map[y][x] = property;
						}
					} else if(((property & Room.TURNER) > 0) && !((property & Room.M_DOWN) > 0)){
						map[y][x] = property;
					} else if(((property & Room.PLAYER) > 0) && ((target & Room.SWAP) > 0)){
						swap(x, y, x, y + 1);
						y++;
					} else if((target & Room.WALL) > 0){
						// blocked
					} else if(target == Room.EMPTY){
						map[y + 1][x] = property;
						map[y][x] = Room.EMPTY;
						y++;
					} else if(((target & (Room.ENEMY | Room.ALLY)) > 0) && !((target & friendly) > 0)){
						if((property & Room.WALL) > 0){
							map[y][x] = property;
							push(x, y, Room.DOWN);
							y++;
						} else {
							map[y][x] = (property | Room.ATTACK) + (((target & Room.PLAYER) > 0) ? Room.KILLER : 0);
							kill(x, y + 1, Room.DOWN);
						}
					}
				}
			} else if(dir == Room.LEFT){
				if(x > 0){
					target = map[y][x - 1];
					if((property & Room.GENERATOR) > 0){
						if(target == Room.EMPTY && ((property & Room.TIMER_0) > 0)){
							map[y][x - 1] = (property & ~(Room.GENERATOR | Room.UP_DOWN_LEFT_RIGHT)) | Room.LEFT | (((property & Room.TURNER) > 0) ? Room.M_LEFT : 0);
							if((property & Room.VIRUS) > 0) map[y][x - 1] |= Room.GENERATOR;
							map[y][x] = (property & ~(Room.UP_DOWN_LEFT_RIGHT)) | Room.LEFT | Room.ATTACK;
							if(generateCallback != null) generateCallback(x, y);
						} else {
							map[y][x] = property;
						}
					} else if(((property & Room.TURNER) > 0) && !((property & Room.M_LEFT) > 0)){
						map[y][x] = property;
					} else if(((property & Room.PLAYER) > 0) && ((target & Room.SWAP) > 0)){
						swap(x, y, x - 1, y);
						x--;
					} else if((target & Room.WALL) > 0){
						// blocked
					} else if(target == Room.EMPTY){
						map[y][x - 1] = property;
						map[y][x] = Room.EMPTY;
						x--;
					} else if(((target & (Room.ENEMY | Room.ALLY)) > 0) && !((target & friendly) > 0)){
						if((property & Room.WALL) > 0){
							map[y][x] = property;
							push(x, y, Room.LEFT);
							x--;
						} else {
							map[y][x] = (property | Room.ATTACK) + (((target & Room.PLAYER) > 0) ? Room.KILLER : 0);
							kill(x - 1, y, Room.LEFT);
						}
					}
				}
			}
			return new Point(x, y);
		}

        /* An unstoppable movement from a property */
		public void push(int x, int y, int dir, Boolean move = true) {
			// can we push?
			if(dir == Room.UP){
				if(y > 1 && map[y - 2][x] == Room.EMPTY){
					if((map[y - 1][x] & Room.PUSHED) > 0){
						if((map[y][x] & Room.TURNER) > 0) map[y][x] &= ~(Room.M_UP_DOWN_LEFT_RIGHT);
						else map[y][x] &= ~(Room.UP_DOWN_LEFT_RIGHT);
						return;
					} else if((map[y - 1][x] & Room.PLAYER) > 0){
						playerPush = Room.compassPoints[Room.NORTH];
					}
					map[y - 1][x] &= ~(Room.UP_DOWN_LEFT_RIGHT | Room.ATTACK);
					map[y - 2][x] = map[y - 1][x] | Room.PUSHED | Room.UP;
				} else {
					kill(x, y - 1, Room.UP);
				}
				if(move){
					map[y - 1][x] = map[y][x];
					map[y][x] = Room.EMPTY;
				} else {
					map[y - 1][x] = Room.EMPTY;
				}
				
			} else if(dir == Room.RIGHT){
				if(x < width - 3 && map[y][x + 2] == Room.EMPTY){
					if((map[y][x + 1] & Room.PUSHED) > 0){
						if((map[y][x] & Room.TURNER) > 0) map[y][x] &= ~(Room.M_UP_DOWN_LEFT_RIGHT);
						else map[y][x] &= ~(Room.UP_DOWN_LEFT_RIGHT);
						return;
					} else if((map[y][x + 1] & Room.PLAYER) > 0){
						playerPush = Room.compassPoints[Room.EAST];
					}
					map[y][x + 1] &= ~(Room.UP_DOWN_LEFT_RIGHT | Room.ATTACK);
					map[y][x + 2] = map[y][x + 1] | Room.PUSHED | Room.RIGHT;
				} else {
					kill(x + 1, y, Room.RIGHT);
				}
				if(move){
					map[y][x + 1] = map[y][x];
					map[y][x] = Room.EMPTY;
				} else {
					map[y][x + 1] = Room.EMPTY;
				}
				
			} else if(dir == Room.DOWN){
				if(y < height - 3 && map[y + 2][x] == Room.EMPTY){
					if((map[y + 1][x] & Room.PUSHED) > 0){
						if((map[y][x] & Room.TURNER) > 0) map[y][x] &= ~(Room.M_UP_DOWN_LEFT_RIGHT);
						else map[y][x] &= ~(Room.UP_DOWN_LEFT_RIGHT);
						return;
					} else if((map[y + 1][x] & Room.PLAYER) > 0){
						playerPush = Room.compassPoints[Room.SOUTH];
					}
					map[y + 1][x] &= ~(Room.UP_DOWN_LEFT_RIGHT | Room.ATTACK);
					map[y + 2][x] = map[y + 1][x] | Room.PUSHED | Room.DOWN;
				} else {
					kill(x, y + 1, Room.DOWN);
				}
				if(move){
					map[y + 1][x] = map[y][x];
					map[y][x] = Room.EMPTY;
				} else {
					map[y + 1][x] = Room.EMPTY;
				}
				
			} else if(dir == Room.LEFT){
				if(x > 1 && map[y][x - 2] == Room.EMPTY){
					if((map[y][x - 1] & Room.PUSHED) > 0){
						if((map[y][x] & Room.TURNER) > 0) map[y][x] &= ~(Room.M_UP_DOWN_LEFT_RIGHT);
						else map[y][x] &= ~(Room.UP_DOWN_LEFT_RIGHT);
						return;
					} else if((map[y][x - 1] & Room.PLAYER) == Room.PLAYER){
						playerPush = Room.compassPoints[Room.WEST];
					}
					map[y][x - 1] &= ~(Room.UP_DOWN_LEFT_RIGHT | Room.ATTACK);
					map[y][x - 2] = map[y][x - 1] | Room.PUSHED | Room.LEFT;
				} else {
					kill(x - 1, y, Room.LEFT);
				}
				if(move){
					map[y][x - 1] = map[y][x];
					map[y][x] = Room.EMPTY;
				} else {
					map[y][x - 1] = Room.EMPTY;
				}
			}
			if(pushCallback != null) pushCallback(x, y, dir);
		}

        /* Wipe action data in an area - used to clear animation data */
		public Array flushStatus(int x, int y, int radius) {
			var list = new Array();
			int fromX = x - radius;
			int fromY = y - radius;
			int length = 1 + radius * 2;
			int toX = fromX + length;
			int toY = fromY + length;
			int r, c, property;
			for(r = fromY; r < toY; r++){
				for(c = fromX; c < toX; c++){
					if(c >= 0 && r >= 0 && c < width && r < height){
						// preserve a turner's previous direction
						property = map[r][c];
						if(!((property & Room.TURNER) > 0)) property &= ~(Room.UP_DOWN_LEFT_RIGHT);
						else property &= ~(Room.M_UP_DOWN_LEFT_RIGHT);
						property &= ~(Room.ATTACK | Room.BLOCKED | Room.PUSHED);
						map[r][c] = property;
					}
				}
			}
			return list;
		}

        public void moveEntities(Array<Point> list) {
			int i, j;
            var target = new Point();
			int px, py;
			int mx, my;
			Array nodes = new Array();
			int dir;
			int best;
			int value;
			int property;
			int dest;
			int type;
			int friendly;
			Point p;
			playerPush = null;
			//trace("list", list);
			for(i = 0; i < list.length; i++){
				p = list[i];
				mx = p.x;
				my = p.y;
				property = map[my][mx];
				// any entity that has already moved must be the player or has been swapped - forfeiting their turn
				// viruses cannot move without a generator
				if(
					property == 0 ||
					(((property & Room.ALLY) > 0) && ((property & Room.UP_DOWN_LEFT_RIGHT) == Room.UP_DOWN_LEFT_RIGHT)) ||
					(property & (Room.VIRUS | Room.GENERATOR)) == Room.VIRUS
				){
					continue;
				}
				friendly = property & (Room.ALLY | Room.ENEMY);
				type = property & (Room.VIRUS | Room.MOVER | Room.TURNER | Room.TRAP | Room.ALLY);
				px = mx - (player.x - ENEMY_ACTIVE_RADIUS);
				py = my - (player.y - ENEMY_ACTIVE_RADIUS);
				best = int.MaxValue;
				dir = 0;
				// advance timers
				if((property & Room.GENERATOR) == Room.GENERATOR){
					property = Room.rotateBits(property, 1, Room.TIMER_MASK, 4);
					map[my][mx] = property;
				}
				if((type & Room.ALLY) == Room.ALLY){
					if(((playerDir & Room.UP) == Room.UP) && my > 0 && !((map[my - 1][mx] & Room.DOOR) == Room.DOOR)){
						p = move(mx, my, Room.UP, friendly);
					} else if(((playerDir & Room.RIGHT) == Room.RIGHT) && mx < width - 1 && !((map[my][mx + 1] & Room.DOOR) == Room.DOOR)){
						p = move(mx, my, Room.RIGHT, friendly);
					} else if(((playerDir & Room.DOWN) == Room.DOWN) && my < height - 1 && !((map[my + 1][mx] & Room.DOOR) == Room.DOOR)){
						p = move(mx, my, Room.DOWN, friendly);
					} else if(((playerDir & Room.LEFT) == Room.LEFT) && mx > 0 && !((map[my][mx - 1] & Room.DOOR) == Room.DOOR)){
						p = move(mx, my, Room.LEFT, friendly);
					}
					if(((map[p.y][p.x] & Room.BLOCKED) == Room.BLOCKED) && blockedCallback != null){
						blockedCallback(p.x, p.y, playerDir);
					}
				} else if(((type & Room.TRAP) == Room.TRAP) && !((property & Room.GENERATOR) == Room.GENERATOR)){
					// traps just hit the player when they are next to their m_dirs
					if(((property & Room.M_UP) == Room.M_UP) && my > 0 && ((map[my - 1][mx] & Room.ALLY) == Room.ALLY)){
						move(mx, my, Room.UP, friendly);
					} else if(((property & Room.M_RIGHT) == Room.M_RIGHT) && mx < width - 1 && ((map[my][mx + 1] & Room.ALLY) == Room.ALLY)){
						move(mx, my, Room.RIGHT, friendly);
					} else if(((property & Room.M_DOWN) == Room.M_DOWN) && my < height - 1 && ((map[my + 1][mx] & Room.ALLY) == Room.ALLY)){
						move(mx, my, Room.DOWN, friendly);
					} else if(((property & Room.M_LEFT) == Room.M_LEFT) && mx > 0 && ((map[my][mx - 1] & Room.ALLY) == Room.ALLY)){
						move(mx, my, Room.LEFT, friendly);
					}
				} else {
					if(py > 0 && my > 0){
						value = pathMap[py - 1][px];
						dest = map[my - 1][mx];
						if(
							my > 0 &&
							!((dest & (Room.WALL | friendly)) > 0) &&
							value != PATH_WALL && value < best &&
							!(((type & Room.MOVER) == Room.MOVER) && !((property & Room.M_UP) == Room.M_UP))
						){
							dir = Room.UP;
							//trace("UP", value);
							best = value;
						}
					}
					if(px < PATH_MAP_SIZE - 1 && mx < width - 1){
						value =  pathMap[py][px + 1];
						dest = map[my][mx + 1];
						if(
							mx < width - 1 &&
							!((dest & (Room.WALL | friendly)) > 0) &&
							value != PATH_WALL && value < best &&
							!(((type & Room.MOVER) == Room.MOVER) && !((property & Room.M_RIGHT) == Room.M_RIGHT))
						){
							dir = Room.RIGHT;
							//trace("RIGHT", value);
							best = value;
						}
					}
					if(py < PATH_MAP_SIZE - 1 && my < height - 1){
						value = pathMap[py + 1][px];
						dest = map[my + 1][mx];
						if(
							my < height - 1 &&
							!((dest & (Room.WALL | friendly)) > 0) &&
							value != PATH_WALL && value < best &&
							!(((type & Room.MOVER) == Room.MOVER) && !((property & Room.M_DOWN) == Room.M_DOWN))
						){
							dir = Room.DOWN;
							//trace("DOWN", value);
							best = value;
						}
					}
					if(px > 0 && mx > 0){
						value = pathMap[py][px - 1];
						dest = map[my][mx - 1];
						if(
							mx > 0 &&
							!((dest & (Room.WALL | friendly)) > 0) &&
							value != PATH_WALL && value < best &&
							!(((type & Room.MOVER) == Room.MOVER) && !((property & Room.M_LEFT) == Room.M_LEFT))
						){
							dir = Room.LEFT;
							//trace("LEFT", value);
							best = value;
						}
					}
					if(dir > 0){
						move(mx, my, dir, friendly);
					}
				}
			}
			// we have to buffer player pushes
			if(playerPush != null){
				player.x += playerPush.x;
				player.y += playerPush.y;
				playerPush = null;
			}
		}

        /* Fills the pathMap via wave propagation */
		public void fillPathMap(int x, int y, Array<Point> points = null, int find = 0, Array<Point> findList = null, int ignore = 0) {
			int mx = x, my = y;
			int px, py;
			int i, d = 0;
            Point p;
			Room.fill(0, 0, PATH_MAP_SIZE, PATH_MAP_SIZE, pathMap, PATH_WALL);
			if(points == null){
				points = new Array<Point> { new Point(ENEMY_ACTIVE_RADIUS, ENEMY_ACTIVE_RADIUS) };
				pathMap[ENEMY_ACTIVE_RADIUS][ENEMY_ACTIVE_RADIUS] = 0;
			} else {
				// reposition points to pathMap
				i = points.length;
				//trace("");
				//trace(points);
				while((i--) > 0){
					p = points[i];
					if((map[p.y][p.x] & (Room.WALL | Room.VOID)) > 0){
						points.splice(i, 1);
						continue;
					}
					p = new Point(p.x - (player.x - ENEMY_ACTIVE_RADIUS), p.y - (player.y - ENEMY_ACTIVE_RADIUS));
					if(p.x < 0 || p.y < 0 || p.x >= PATH_MAP_SIZE || p.y >= PATH_MAP_SIZE){
						points.splice(i, 1);
						continue;
					}
					points[i] = p;
					pathMap[p.y][p.x] = 0;
				}
			}
			int length = points.length;
			// ah le double while loop - makes its point though dunnit?
			while((length) > 0) {
				while((length--) > 0){
					p = points.shift();
					px = p.x;
					py = p.y;
					mx = x + px - ENEMY_ACTIVE_RADIUS;
					my = y + py - ENEMY_ACTIVE_RADIUS;
					if(my > 0 && py > 0){
						if(pathMap[py - 1][px] == PATH_WALL){
							if(!((map[my - 1][mx] & (Room.WALL | Room.VOID)) > 0)){
								points.push(new Point(px, py - 1));
							}
							if(find > 0 && findList != null){
								if(((map[my - 1][mx] & find) == find) && !((map[my - 1][mx] & ignore) > 0)){
									findList.push(new Point(mx, my - 1));
								}
							}
							pathMap[py - 1][px] = d + 1;
						}
					}
					if(mx < width - 1 && px < PATH_MAP_SIZE - 1){
						if(pathMap[py][px + 1] == PATH_WALL){
							if(!((map[my][mx + 1] & (Room.WALL | Room.VOID)) > 0)){
								points.push(new Point(px + 1, py));
							}
							if(find > 0 && findList != null){
								if(((map[my][mx + 1] & find) == find) && !((map[my][mx + 1] & ignore) > 0)){
									findList.push(new Point(mx + 1, my));
								}
							}
							pathMap[py][px + 1] = d + 1;
						}
					}
					if(my < height - 1 && py < PATH_MAP_SIZE - 1){
						if(pathMap[py + 1][px] == PATH_WALL){
							if(!((map[my + 1][mx] & (Room.WALL | Room.VOID)) > 0)){
								points.push(new Point(px, py + 1));
							}
							if(find > 0 && findList != null){
								if(((map[my + 1][mx] & find) == find) && !((map[my + 1][mx] & ignore) > 0)){
									findList.push(new Point(mx, my + 1));
								}
							}
							pathMap[py + 1][px] = d + 1;
						}
					}
					if(mx > 0 && px > 0){
						if(pathMap[py][px - 1] == PATH_WALL){
							if(!((map[my][mx - 1] & (Room.WALL | Room.VOID)) > 0)){
								points.push(new Point(px - 1, py));
							}
							if(find > 0 && findList != null){
								if(((map[my][mx - 1] & find) == find) && !((map[my][mx - 1] & ignore) > 0)){
									findList.push(new Point(mx - 1, my));
								}
							}
							pathMap[py][px - 1] = d + 1;
						}
					}
				}
				length = points.length;
				d++;
			}
		}

        /* Called when killing a property at a tile - killCallback must accept the location and direction */
		public void kill(int x, int y, int dir, int explosion = 0) {
			int property = map[y][x];
			// player receives food for kills next to them
			int vx = x - player.x;
			int vy = y - player.y;
			if(
				(vx == 0 && (vy == 1 || vy == -1)) ||
				(vy == 0 && (vx == 1 || vx == -1))
			){
				food += FOOD_KILL;
				if(food > FOOD_MAX) food = FOOD_MAX;
			}
			// doors create new rooms
			if((property & Room.DOOR) == Room.DOOR){
				if((property & Room.ENDING) == Room.ENDING){
					ending(x, y, dir);
				} else {
					int value = 0;
					if((property & Room.INCREMENT) == Room.INCREMENT) value++;
					Point p = createRoom(x, y, dir, value);
					x = p.x;
					y = p.y;
				}
			}
			if(killCallback != null) killCallback(x, y, dir, property, (explosion > 0 || (((property & Room.BOMB) == Room.BOMB)) ? 1 : 0));
			map[y][x] = Room.EMPTY;
			if((property & Room.BOMB) == Room.BOMB){
				explode(x, y, dir, explosion);
			}
		}
		
		public void ending(int x, int y, int dir) {
			ended = true;
			if(endingCallback != null) endingCallback(x, y, dir);
		}

        /* Create a room adjacent to the one the player is in */
		public Point createRoom(int x, int y, int dir, int dist) {
			int roomsWide = (int)Math.Ceiling((double)width / room.width);
			int roomsHigh = (int)Math.Ceiling((double)height / room.height);
			// current quadrant
			int qx = player.x / room.width;
			int qy = player.y / room.height;
			Rectangle qRect = new Rectangle((qx * room.width) - qx, (qy * room.width) - qy, room.width, room.height);
			// target quadrant
			int tx = qx;
			int ty = qy;
			if(dir == Room.UP) ty--;
			else if(dir == Room.RIGHT) tx++;
			else if(dir == Room.DOWN) ty++;
			else if(dir == Room.LEFT) tx--;
			// move current room if target out of bounds
			if(tx < 0 || tx >= roomsWide || ty < 0 || ty >= roomsHigh){
				playerPush = new Point();
				if(ty < 0){
					qy++;
					ty++;
					y += room.height - 1;
					playerPush.y += room.height - 1;
				} else if(tx >= roomsWide){
					qx--;
					tx--;
					x -= room.width - 1;
					playerPush.x -= room.width - 1;
				} else if(ty >= roomsHigh){
					qy--;
					ty--;
					y -= room.height - 1;
					playerPush.y -= room.height - 1;
				} else if(tx < 0){
					qx++;
					tx++;
					x += room.width - 1;
					playerPush.x += room.width - 1;
				}
				//trace("moved to", qx, qy, tx, ty);
				//trace("at", (qx * room.width) - qx, (qy * room.width) - qy);
				Room.copyRectTo(map, qRect, map, (qx * room.width) - qx, (qy * room.height) - qy, copyBuffer);
			}
			// erase unused side
			int revealDir = 0;
			int eraseDir = 0;
			if(qx == tx){
				if(qx == 0) eraseDir = Room.EAST;
				else if(qx == 1) eraseDir = Room.WEST;
				if(qy == 0) revealDir = Room.SOUTH;
				else if(qy == 1) revealDir = Room.NORTH;
			} else if(qy == ty){
				if(qy == 0) eraseDir = Room.SOUTH;
				else if(qy == 1) eraseDir = Room.NORTH;
				if(qx == 0) revealDir = Room.EAST;
				else if(qx == 1) revealDir = Room.WEST;
			}
			if(displaceCallback != null){
				int pushX = 0, pushY = 0;
				if(playerPush != null){
					pushX = playerPush.x;
					pushY = playerPush.y;
				}
				displaceCallback(pushX, pushY, revealDir, eraseDir);
			}
			
			if(eraseDir == Room.NORTH) Room.fill(0, 0, width, room.height - 1, map, Room.VOID);
			else if(eraseDir == Room.EAST) Room.fill(room.width, 0, room.width - 1, height, map, Room.VOID);
			else if(eraseDir == Room.SOUTH) Room.fill(0, room.height, width, room.height - 1, map, Room.VOID);
			else if(eraseDir == Room.WEST) Room.fill(0, 0, room.width - 1, height, map, Room.VOID);
			// create and paste
			room.endingDist -= dist;
			if(room.endingDist < 1) room.endingDist = 1;
			if(room.endingDist > Room.DIST_TO_ENDING) room.endingDist = Room.DIST_TO_ENDING;
			room.init(new Point(x - ((qx * room.width) - qx), y - ((qy * room.height) - qy)), revealDir);
			room.copyTo(map, (tx * room.width) - tx, (ty * room.height) - ty);
			return new Point(x, y);
		}

        /* Create a mask for hiding unvisitable parts of a puzzle level
		 * wave propagation used here to spread visibility from the Ending and stopping on indestrucible walls */
		public Array<Array<int>> getBlackOutMap() {
			// get ending position
			var endings = new Array<Point>();
			Room.setPropertyLocations(0, 0, width, height, map, Room.ENDING, endings);
			if(endings.length == 0) return null;
			int i, x, y;
			Point p = new Point(endings[0].x, endings[0].y);
			var blackOutMap = Room.create2DArray(width, height, 0);
			Array<Point> points = new Array<Point> { p };
			int length = points.length;
			blackOutMap[p.y][p.x] = 1;
			while(length > 0){
				while((length--) > 0){
					p = points.shift();
					x = p.x;
					y = p.y;
					if((map[y][x] & Room.INDESTRUCTIBLE) == Room.INDESTRUCTIBLE) continue;
					// cardinals
					if(y > 0 && blackOutMap[y - 1][x] == 0){
						points.push(new Point(x, y - 1));
						blackOutMap[y - 1][x] = 1;
					}
					if(x < width - 1 && blackOutMap[y][x + 1] == 0){
						points.push(new Point(x + 1, y));
						blackOutMap[y][x + 1] = 1;
					}
					if(y < height - 1 && blackOutMap[y + 1][x] == 0){
						points.push(new Point(x, y + 1));
						blackOutMap[y + 1][x] = 1;
					}
					if(x > 0 && blackOutMap[y][x - 1] == 0){
						points.push(new Point(x - 1, y));
						blackOutMap[y][x - 1] = 1;
					}
					// corners
					if(y > 0 && x > 0 && blackOutMap[y - 1][x - 1] == 0){
						points.push(new Point(x - 1, y - 1));
						blackOutMap[y - 1][x - 1] = 1;
					}
					if(y > 0 && x < width - 1 && blackOutMap[y - 1][x + 1] == 0){
						points.push(new Point(x + 1, y - 1));
						blackOutMap[y - 1][x + 1] = 1;
					}
					if(y < height - 1 && x < width - 1 && blackOutMap[y + 1][x + 1] == 0){
						points.push(new Point(x + 1, y + 1));
						blackOutMap[y + 1][x + 1] = 1;
					}
					if(y < height - 1 && x > 0 && blackOutMap[y + 1][x - 1] == 0){
						points.push(new Point(x - 1, y + 1));
						blackOutMap[y + 1][x - 1] = 1;
					}
				}
				length = points.length;
			}
			
			// if the player is blacked-out, the blackout map is useless
			if(blackOutMap[player.y][player.x] == 0) return null;
			
			return blackOutMap;
		}

        public static void printPathMap() {
			int r, c;
            String str;
			int size = pathMap.length;
			String line = "";
			for(r = 0; r < size; r++){
				line = "";
				for(c = 0; c < size; c++){
					str = pathMap[r][c] + "";
					if(str.Length < 2) str = "0" + str;
					if(c < size - 1) str += ",";
					line += str;
				}
				language.trace(line);
			}
			language.trace("");
		}
    }
}
