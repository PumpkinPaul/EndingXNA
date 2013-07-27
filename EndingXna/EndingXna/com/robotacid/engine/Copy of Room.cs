//using System;
//using System.Collections.Generic;
//using flash;
//using XorRandom = com.robotacid.util.XorRandom;
//using Point = flash.geom.Point;
//using Rectangle = flash.geom.Rectangle;

//namespace com.robotacid.engine
//{
//    /// <summary>
//    /// A template generator for rooms in the game
//    ///
//    /// The data is designed so that all possible game states are bitwise flags on a 2D array.
//    /// This keeps the scope small and portable. No good puzzle ever needed clutter.
//    ///
//    /// PUZZLE rooms are limited to one grid with one exit
//    /// ADVENTURE rooms are journeys between chains of rooms
//    /// 
//    /// @author Aaron Steed, robotacid.com
//    /// @conversion Paul Cunningham, pumpkin-games.net
//    /// </summary>
//    public class Room  {

//        public int type;
//        public int width;
//        public int height;
//        public int startX;
//        public int startY;
//        public int[][] map;
//        public Point[] doors;
//        public int roomTurns;
//        public int vertHalfway;
//        public int horizHalfway;
//        public int endingDist;

//        public static XorRandom random;
		
//        // property constants
//        public const int EMPTY = 0;
//        // directions
//        public const int UP = 1 << 0;
//        public const int RIGHT = 1 << 1;
//        public const int DOWN = 1 << 2;
//        public const int LEFT = 1 << 3;
//        // direction memory states
//        public const int M_UP = 1 << 4;
//        public const int M_RIGHT = 1 << 5;
//        public const int M_DOWN = 1 << 6;
//        public const int M_LEFT = 1 << 7;
//        // properties
//        public const int ATTACK = 1 << 8;
//        public const int BLOCKED = 1 << 9;
//        public const int PUSHED = 1 << 18;
//        public const int GENERATOR = 1 << 21;
//        public const int BOMB = 1 << 28;
//        public const int SWAP = 1 << 20;
//        public const int VOID = 1 << 27;
//        public const int WALL = 1 << 10;
//        public const int INDESTRUCTIBLE = 1 << 29;
//        public const int TIMER_0 = 1 << 22;
//        public const int TIMER_1 = 1 << 23;
//        public const int TIMER_2 = 1 << 24;
//        public const int TIMER_3 = 1 << 25;
//        public const int ENEMY = 1 << 12;
//        public const int ALLY = 1 << 26;
//        public const int PLAYER = 1 << 11;
//        public const int DOOR = 1 << 13;
//        public const int VIRUS = 1 << 30;
//        public const int TRAP = 1 << 19;
//        public const int TURNER = 1 << 17;
//        public const int MOVER = 1 << 14;
//        public const int KILLER = 1 << 15;
//        public const int INCREMENT = 1 << 16;
//        public const int ENDING = 1 << 31;
//        // combo constants
//        public const int UP_DOWN_LEFT_RIGHT = 15;
//        public static int M_UP_DOWN_LEFT_RIGHT;
//        // utils
//        public static int TIMER_MASK;
//        public const int M_DIR_SHIFT = 4;
		
//        // types
//        public const int PUZZLE = 0;
//        public const int ADVENTURE = 1;
		
//        // compass
//        public const int NORTH = 0;
//        public const int EAST = 1;
//        public const int SOUTH = 2;
//        public const int WEST = 3;
//        public int[] compass = {UP, RIGHT, DOWN, LEFT};
//        //public const Object toCompass = {
//        //    1:0,
//        //    2:1,
//        //    4:2,
//        //    8:3
//        //};
//        public readonly Point[] compassPoints = {new Point(0, -1), new Point(1, 0), new Point(0, 1), new Point( -1, 0)};
		
//        public const int DIST_TO_ENDING = 10;

//        public Room(int type, int width, int height) {
//            this.type = type;
//            this.width = width;
//            this.height = height;
//            map = create2DArray(width, height, WALL | INDESTRUCTIBLE);
//            startPosition();
//        }
//        public static void init() {
//            M_UP_DOWN_LEFT_RIGHT = (M_UP | M_DOWN | M_LEFT | M_RIGHT);
//            TIMER_MASK = TIMER_0 | TIMER_1 | TIMER_2 | TIMER_3;
//        }

//        /* Creates the initial template room */
//        public void startPosition() {
//            clear();
//            startX = (int)(width * 0.5);
//            startY = (int)(height * 0.5);
//            doors = new [] {
//                new Point(startX, 0),
//                new Point(width - 1, startY),
//                new Point(startX, height - 1),
//                new Point(0, startY)
//            };
//            horizHalfway = startX;
//            vertHalfway = startY;
//            roomTurns = 0;
//            endingDist = DIST_TO_ENDING;
//            random = new XorRandom();
//        }

//        public void init(Point door = null, int revealDir = -1) {
//            if(type == ADVENTURE){
//                adventureFill(door, revealDir);
//            } else if(type == PUZZLE){
//                clear();
//            }
//            roomTurns++;
//        }
		
//        public void clear() {
//            fill(0, 0, width, height, map, WALL | INDESTRUCTIBLE);
//            fill(1, 1, width - 2, height - 2, map, EMPTY);
//        }

//        /* Get where the doors should be in a new room (accounting for a door destroyed as well) */
//        public Point[] getDoors(Point door = null, int revealDir = -1) {
//            int i;
//            Point p;
//            int value;
//            int skipDir = -1;
//            Point[] doors = this.doors.slice();
//            if(revealDir == NORTH){
//                skipDir = SOUTH;
//                door.y = height - 1;
//            } else if(revealDir == EAST){
//                skipDir = WEST;
//                door.x = 0;
//            } else if(revealDir == SOUTH){
//                skipDir = NORTH;
//                door.y = 0;
//            } else if(revealDir == WEST){
//                skipDir = EAST;
//                door.x = width - 1;
//            }
//            int[] dists = {0, 0, 0, 0};
//            int length;
//            int farthestLength = 0;
//            int farthestIndex = -1;
//            int closestLength = int.MaxValue;
//            int closestIndex = -1;
//            // randomise door positions
//            if(skipDir > -1){
//                for(i = 0; i < 4; i++){
//                    if(i == skipDir){
//                        dists[i] = 0;
//                    } else {
//                        p = doors[i];
//                        if(i == NORTH || i == SOUTH) doors[i].x = 1 + random.rangeInt(width - 2);
//                        else if(i == EAST || i == WEST) doors[i].y = 1 + random.rangeInt(height - 2);
//                        length = (int)(Math.Abs(p.x - door.x) + Math.Abs(p.y - door.y));
//                        if(length > farthestLength){
//                            farthestLength = length;
//                            farthestIndex = i;
//                        }
//                        if(length < closestLength){
//                            closestLength = length;
//                            closestIndex = i;
//                        }
//                    }
//                }
//                // the ending is always the farthest away from the door entered
//                dists[farthestIndex] = INCREMENT;
//            } else {
//                // the inital room must only show EAST or WEST - 1st door must always be visible
//                dists[(random.coinFlip() ? EAST : WEST)] = INCREMENT;
//            }
//            for(i = 0; i < 4; i++){
//                value = dists[i];
//                if(i == skipDir){
//                    doors[i] = door;
//                    continue;
//                }
//                p = doors[i];
//                map[p.y][p.x] = DOOR | ENEMY | (1 << (i + M_DIR_SHIFT));
//                if(endingDist <= 2 && value == INCREMENT) value = ENDING;
//                map[p.y][p.x] |= value;
//            }
//            return doors;
//        }

//        /* Dig routes between the door ways - removes anything that places the path in check */
//        public void connectDoors(Point[] doors) {
//            Point p;
//            int r, c, x, y;
//            // N S
//            x = doors[NORTH].x;
//            p = doors[SOUTH];
//            for(r = 1; r < height - 1; r++){
//                if(r == 1 || r == height - 2 || ((map[r][x] & WALL) == WALL)) map[r][x] = EMPTY;
//                clearCheck(x, r);
//                if(r == vertHalfway){
//                    while(x != p.x){
//                        if(x < p.x) x++;
//                        if(x > p.x) x--;
//                        if((map[r][x] & WALL) == WALL) map[r][x] = EMPTY;
//                        clearCheck(x, r);
//                    }
//                }
//            }
//            // W E
//            y = doors[WEST].y;
//            p = doors[EAST];
//            for(c = 1; c < width - 1; c++){
//                if(c == 1 || c == width - 2 || ((map[y][c] & WALL) == WALL)) map[y][c] = EMPTY;
//                clearCheck(c, y);
//                if(c == horizHalfway){
//                    while(y != p.y){
//                        if(y < p.y) y++;
//                        if(y > p.y) y--;
//                        if((map[y][c] & WALL) == WALL) map[y][c] = EMPTY;
//                        clearCheck(c, y);
//                    }
//                }
//            }
//            horizHalfway = 1 + random.rangeInt(width - 3);
//            vertHalfway = 1 + random.rangeInt(height - 3);
//        }

//        /* Remove any enemies surrounding a space that would endanger it */
//        public void clearCheck(int x, int y) {
//            if(y > 0 && ((map[y - 1][x] & ENEMY) == ENEMY) && (
//                    (
//                        ((map[y - 1][x] & TURNER) == TURNER) && ((map[y - 1][x] & DOWN) == DOWN)
//                    ) || (
//                        ((map[y - 1][x] & (TRAP | MOVER)) == (TRAP | MOVER) ) && ((map[y - 1][x] & M_DOWN) == M_DOWN)
//                    ) || (
//                        ((map[y - 1][x] & GENERATOR) == GENERATOR)
//                    )
//                )
//            ){
//                map[y - 1][x] = EMPTY;
//            }
//            if(x < width - 1 && ((map[y][x + 1] & ENEMY) == ENEMY) && (
//                    (
//                        ((map[y][x + 1] & TURNER) == TURNER) && ((map[y][x + 1] & LEFT) == LEFT)
//                    ) || (
//                        ((map[y][x + 1] & (TRAP | MOVER))  == (TRAP | MOVER)) && ((map[y][x + 1] & M_LEFT) == M_LEFT)
//                    ) || (
//                        ((map[y][x + 1] & GENERATOR) == GENERATOR)
//                    )
//                )
//            ){
//                map[y][x + 1] = EMPTY;
//            }
//            if(y < height - 1 && ((map[y + 1][x] & ENEMY) == ENEMY) && (
//                    (
//                        ((map[y + 1][x] & TURNER) == TURNER) && ((map[y + 1][x] & UP) == UP)
//                    ) || (
//                        ((map[y + 1][x] & (TRAP | MOVER)) == (TRAP | TURNER)) && ((map[y + 1][x] & M_UP) == M_UP)
//                    ) || (
//                        ((map[y + 1][x] & GENERATOR) == GENERATOR)
//                    )
//                )
//            ){
//                map[y + 1][x] = EMPTY;
//            }
//            if(x > 0 && ((map[y][x - 1] & ENEMY) == ENEMY) && (
//                    (
//                        ((map[y][x - 1] & TURNER) == TURNER) && ((map[y][x - 1] & RIGHT) == RIGHT)
//                    ) || (
//                        ((map[y][x - 1] & (TRAP | MOVER)) == (TRAP | MOVER)) && ((map[y][x - 1] & M_RIGHT) == M_RIGHT)
//                    ) || (
//                        ((map[y][x - 1] & GENERATOR) == GENERATOR)
//                    )
//                )
//            ){
//                map[y][x - 1] = EMPTY;
//            }
//        }

//        /* Create indestructible walls around unreachable areas and fill insides with VOID */
//        public void fillVoid(Point entry) {
//            Point p = entry;
//            var voidMap = create2DArray(width, height, 0);
//            int i;
//            int x;
//            int y;
//            var points = new List<Point> {p};
//            int property = 0;
//            int length = points.Count;
//            voidMap[p.y][p.x] = 1;
			
//            while(length > 0){
//                while((length--) > 0){
//                    p = points.shift();
//                    x = p.x;
//                    y = p.y;
//                    property = map[y][x];
//                    if(((property & WALL) == WALL) && !((property & ENEMY) == ENEMY)) continue;
//                    // cardinals
//                    if(y > 0 && voidMap[y - 1][x] == 0){
//                        points.push(new Point(x, y - 1));
//                        voidMap[y - 1][x] = 1;
//                    }
//                    if(x < width - 1 && voidMap[y][x + 1] == 0){
//                        points.push(new Point(x + 1, y));
//                        voidMap[y][x + 1] = 1;
//                    }
//                    if(y < height - 1 && voidMap[y + 1][x] == 0){
//                        points.push(new Point(x, y + 1));
//                        voidMap[y + 1][x] = 1;
//                    }
//                    if(x > 0 && voidMap[y][x - 1] == 0){
//                        points.push(new Point(x - 1, y));
//                        voidMap[y][x - 1] = 1;
//                    }
//                }
//                length = points.Count;
//            }
//            int r, c;
//            for(r = 0; r < height; r++){
//                for(c = 0; c < width; c++){
//                    if(voidMap[r][c] == 0 && !((map[r][c] & INDESTRUCTIBLE) == INDESTRUCTIBLE)){
//                        map[r][c] = VOID;
//                    }
//                    if(voidMap[r][c] == 1 && ((property & WALL) == WALL) && !((property & ENEMY) == ENEMY) &&
//                        (
//                            (r > 0 && voidMap[r - 1][c] == 0) ||
//                            (c < width - 1 && voidMap[r][c + 1] == 0) ||
//                            (r < height - 1 && voidMap[r + 1][c] == 0) ||
//                            (c > 0 && voidMap[r][c - 1] == 0)
//                        )
//                    ){
//                        map[r][c] = WALL | INDESTRUCTIBLE;
//                    }
//                }
//            }
//        }
		
//        public void copyTo(int[][] target, int tx, int ty) {
//            int r, c;
//            for(r = 0; r < height; r++){
//                for(c = 0; c < width; c++){
//                    target[ty + r][tx + c] = map[r][c];
//                }
//            }
//        }

//        public static void copyRectTo(int[][] source, Rectangle rect, int[][] target, int tx, int ty, int[][] buffer) {
//            int r, c;
//            // buffering to account for any overlap
//            for(r = 0; r < rect.height; r++){
//                for(c = 0; c < rect.width; c++){
//                    buffer[r][c] = source[rect.y + r][rect.x + c];
//                }
//            }
//            for(r = 0; r < rect.height; r++){
//                for(c = 0; c < rect.width; c++){
//                    target[ty + r][tx + c] = buffer[r][c];
//                }
//            }
//        }

//        /* Used to clear out a section of a grid or flood it with a particular tile type */
//        public static void fill(int x, int y, int width, int height, int[][] target, int index) {
//            int r, c;
//            for(r = y; r < y + height; r++){
//                for(c = x; c < x + width; c++){
//                    target[r][c] = index;
//                }
//            }
//        }

//        /* Scatter some tiles with Math.random */
//        public static void scatterFill(int x, int y, int width, int height, int[][] target, int index, int total, int[]rotations = null, int add = 0) {
//            int r, c;
//            int rot = 0;
//            int breaker = 0;
//            while((total--) > 0){
//                c = x + random.rangeInt(width);
//                r = y + random.rangeInt(height);
//                if(add > 0){
//                    if((target[r][c] & add) == add) target[r][c] |= index;
//                    else{
//                        total++;
//                        if(breaker++ > 200) break;
//                        continue;
//                    }
//                } else target[r][c] = index;
//                if(rotations != null){
//                    target[r][c] |= rotations[rot];
//                    rot++;
//                    if(rot >= rotations.Length) rot = 0;
//                }
//            }
//        }

//        /* Put down a tile - avoiding certain tiles */
//        public static void placeRandom(int x, int y, int width, int height, int[][] target, int index, int avoid = 0) {
//            int r, c;
//            int breaker = 0;
//            do{
//                c = x + random.rangeInt(width);
//                r = y + random.rangeInt(height);
//            } while(((target[r][c] & avoid) == avoid) && breaker++ < width + height);
//            if(breaker < width + height) target[r][c] = index;
//        }

//        /* Scatter some tiles with Math.random */
//        public static void scatterFillList(int x, int y, int width, int height, int[][] target, int[] list) {
//            int i, r, c;
//            int rot;
//            int breaker = 0;
//            for(i = 0; i < list.Length; i++){
//                c = x + random.rangeInt(width);
//                r = y + random.rangeInt(height);
//                target[r][c] = list[i];
//            }
//        }

//        /* Scatter some strips with Math.random */
//        public static void scatterStrips(int x, int y, int width, int height, int[][] target, int index, int total, int xStrip = 0, int yStrip = 0) {
//            int r, c;
//            //trace(xStrip, yStrip);
//            int yRepeat, xRepeat;
//            while((total--) > 0){
//                if(random.coinFlip()){
//                    xRepeat = 1 + random.rangeInt(xStrip - 1);
//                    yRepeat = 0;
//                } else {
//                    xRepeat = 0;
//                    yRepeat = 1 + random.rangeInt(yStrip - 1);
//                }
//                c = -xRepeat + x + random.rangeInt(width);
//                r = -yRepeat + y + random.rangeInt(height);
//                if(r >= x && c >= y && r < y + height && c < x + width){
//                    while(xRepeat > 0 || yRepeat > 0){
//                        target[r][c] = index;
//                        if(xRepeat > 0){
//                            xRepeat--;
//                            if(c < x + width - 1) c++;
//                            else xRepeat = 0;
//                        } else if(yRepeat > 0){
//                            yRepeat--;
//                            if(r < y + height - 1) r++;
//                            else yRepeat = 0;
//                        }
//                    }
//                }
//            }
//        }

//        public static int[][] create2DArray(int width, int height, int basex = -1) {
//            int r, c;
//            int[][] a = {};
//            for(r = 0; r < height; r++){
//                a[r] = new int[] {};
//                for(c = 0; c < width; c++){
//                    a[r][c] = basex;
//                }
//            }
//            return a;
//        }

//        public void randomiseArray(int[] a) {
//            for(var x:*, j:int, i:int = a.length; i; j = random.rangeInt(i), x = a[--i], a[i] = a[j], a[j] = x){}
//        }

//        /* Cyclically shift bits within a given range  - use a minus value for amount to shift left */
//        public static int rotateBits(int n, int amount, int rangeMask, int rangeMaskWidth) {
//            int nRangeMasked = n & ~(rangeMask);
//            n &= rangeMask;
//            if(amount > 0){
//                int absAmount = (amount > 0 ? amount : -amount) % rangeMaskWidth;
//                if(amount < 0){
//                    n = (n << absAmount) | (n >> (rangeMaskWidth - absAmount));
//                } else if(amount > 0){
//                    n = (n >> absAmount) | (n << (rangeMaskWidth - absAmount));
//                }
//            }
//            return (n & rangeMask) | nRangeMasked;
//        }

//        public static Point[] setPropertyLocations(int x, int y, int width, int height, int[][] map, int property, List<Point> locations, List<int> properties = null, int ignore = 0) {
//            var list = new Point[0];
//            int h = map.Length;
//            int w = map[0].Length;
//            int toX = x + width;
//            int toY = y + height;
//            int r, c;
//            for(r = y; r < toY; r++){
//                for(c = x; c < toX; c++){
//                    if(c >= 0 && r >= 0 && c < w && r < h && ((map[r][c] & property) == property) && !((map[r][c] & ignore) == ignore)) {
//                        locations.push(new Point(c, r));
//                        if(properties != null) properties.push(map[r][c]);
//                    }
//                }
//            }
//            return list;
//        }

//        /* Creates a random room with enemies and doors to escape towards an ending
//         * - 
//         * also used for debugging
//         */
//        public void adventureFill(Point door = null, int revealDir = -1) {
//            clear();
//            var doors = getDoors(door, revealDir);
//            var walls = random.rangeInt(width + height) + width + height;
//            scatterFill(1, 1, width - 2, height - 2, map, WALL, walls);
//            //scatterStrips(1, 1, width - 2, height - 2, map, WALL, (roomTurns % 10) + (width + height) * 2, 2 + random.rangeInt(2), 2 + random.rangeInt(2));
//            //fill(1, 1, width - 2, height - 2, map, WALL);
			
//            var recipes = new [] {
//                ENEMY | VIRUS,
//                ENEMY | TRAP,
//                ENEMY | TURNER,
//                ENEMY | MOVER,
//                ENEMY | MOVER | M_LEFT | M_RIGHT | M_UP | M_DOWN,
//                ALLY | SWAP,
//                SWAP | WALL
//            };
//            // when changing the recipe list, update this index
//            int allyIndex = 5;
			
//            var enemySpice = new [] {
//                0,
//                GENERATOR | TIMER_3,
//                WALL,
//                BOMB,
//                BOMB | WALL,
//                GENERATOR | TIMER_3 | WALL,
//                BOMB | GENERATOR | TIMER_3,
//                BOMB | GENERATOR | TIMER_3 | WALL
//            };
			
//            // change to 0 for a blank room to debug in
//            int total = width + height + random.rangeInt(width);
//            int turns;
//            int dir;
//            int compassPos = NORTH;
//            int item;
//            int bombSpice = 1 << 0;
//            int wallSpice = 1 << 1;
//            int generatorSpice = 1 << 2;
//            int recipeRange = Math.Min((DIST_TO_ENDING - endingDist) + 3, recipes.Length);
//            int spiceRange = Math.Min((DIST_TO_ENDING - endingDist) + 1, enemySpice.Length);
//            int spice;
//            int n;
//            int generatorVirii = 0;
			
//            while((total--) > 0){
//                n = random.rangeInt(recipeRange);
//                // reroll swap recipes every second pick
//                while(
//                    (((total & 1) == 1) && ((recipes[n] & SWAP) == SWAP))
//                ){
//                    n = random.rangeInt(recipeRange);
//                }
//                item = recipes[n];
//                if((item & ENEMY) == ENEMY){
//                    if((item & TURNER) == TURNER){
//                        dir = compass[random.rangeInt(compass.Length)];
//                        item |= dir;
//                    } else if((item & TRAP) == TRAP){
//                        dir = 1 + random.rangeInt(15);
//                        dir <<= M_DIR_SHIFT;
//                        item |= dir;
//                    } else if((item & MOVER) == MOVER){
//                        if(!((item & M_UP_DOWN_LEFT_RIGHT) == M_UP_DOWN_LEFT_RIGHT)){
//                            item |= random.coinFlip() ? (M_UP | M_DOWN) : (M_LEFT | M_RIGHT);
//                        }
//                    }
//                    if(random.coinFlip()){
//                        spice = random.rangeInt(spiceRange);
//                        if(((enemySpice[spice] & GENERATOR) == GENERATOR) && ((item & VIRUS) == VIRUS)){
//                            if(generatorVirii == 0) item |= enemySpice[spice];
//                            generatorVirii++;
//                        } else {
//                            item |= enemySpice[spice];
//                        }
//                    }
//                } else if((item & SWAP) == SWAP){
//                    spice = random.rangeInt(spiceRange);
//                    if((enemySpice[spice] & BOMB) == BOMB){
//                        item |= BOMB;
//                    }
//                }
//                //item = VIRUS | ENEMY;
//                placeRandom(1, 1, width - 2, height - 2, map, item, WALL);
//            }
//            // clear a path to the doors
//            connectDoors(doors);
			
//            fillVoid(revealDir == - 1 ?  new Point(startX, startY) : doors[revealDir]);
			
//            // debugging recipes:
//            //map[startY][startX + 1] = ENEMY | VIRUS;
//            //map[startY + 1][startX + 1] = ENEMY | VIRUS;
//            //map[startY][startX + 3] = ENEMY | VIRUS;
//            //map[startY + 1][startX + 3] = ALLY | SWAP;
//            //map[startY + 3][startX] = ENEMY | WALL | VIRUS | GENERATOR | TIMER_3;
//            //map[startY][startX + 2] = ENEMY | MOVER | BOMB;
//            //map[startY][startX + 3] = ENEMY | MOVER | BOMB;
//            //map[startY][startX + 1] = WALL | SWAP;
//            //map[startY][startX + 2] = ALLY | SWAP;
//            //map[startY][startX + 3] = ALLY | SWAP;
//            //map[startY][startX] = ENEMY | TURNER | UP | WALL | GENERATOR | TIMER_3;
//            //map[startY][startX] = ENEMY | TURNER | RIGHT | WALL;
//        }
		
//        public static int oppositeDirection(int dir){
//            if((dir & UP) == UP) return DOWN;
//            else if((dir & RIGHT) == RIGHT) return LEFT;
//            else if((dir & DOWN) == DOWN) return UP;
//            else if((dir & LEFT) == LEFT) return RIGHT;
//            return 0;
//        }
//    }
//}
