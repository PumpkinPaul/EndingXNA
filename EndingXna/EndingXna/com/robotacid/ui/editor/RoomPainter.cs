using System;
using com.robotacid.engine;
using com.robotacid.gfx;
using flash.geom;

namespace com.robotacid.ui.editor
{
    /// <summary>
    /// Edits the Room object via user interaction
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class RoomPainter {

        public static Game game;
		public static Renderer renderer;
     		
		public Boolean active;
		public int px;
		public int py;
		public RoomPalette palette;
		public LevelData data;
		public Level level;
		
		private int paintCol;
		private Point endingPos;
		// paint cols
		private const int PAINT = 0;
		private const int ILLEGAL = 1;

        public RoomPainter(Level level, LevelData data) {
			this.level = level;
			this.data = data;
			active = false;
			palette = new RoomPalette(level);
			endingPos = new Point();
		}

        public void main() {
            palette.main();
            px = (int)((renderer.canvas.mouseX - renderer.canvasPoint.x) * Game.INV_SCALE);
            py = (int)((renderer.canvas.mouseY - renderer.canvasPoint.y) * Game.INV_SCALE);
            if(!palette.uiManager.mouseLock && !renderer.camera.dragScroll && palette.uiManager.currentGroup == 0){
                if(game.mousePressed){
                    paintCol = PAINT;
                    if(px >= 0 && py >= 0 && px < data.width && py < data.height){
                        if(
                            px == 0 ||
                            py == 0 ||
                            px == data.width - 1 ||
                            py == data.height -1 ||
                            (data.room.type == Room.ADVENTURE &&
                                (
                                    px == (int)(data.width * 0.5) >> 0 ||
                                    py == (int)(data.height * 0.5) >> 0
                                )
                            ) ||
                            (((palette.property & Room.PLAYER) == Room.PLAYER) && game.mousePressedCount != game.frameCount) ||
                            ((data.map[py][px] & Room.VOID) == Room.VOID)
                        ){
                            // set ending in puzzle mode
                            if(
                                data.room.type == Room.PUZZLE &&
                                (
                                    px == 0 ||
                                    py == 0 ||
                                    px == data.width - 1 ||
                                    py == data.height -1
                                ) && !(
                                    px == py ||
                                    (px == data.width - 1 && py == 0) ||
                                    (px == 0 && py == data.height - 1)
                                )
                            ){
                                data.map[(int)endingPos.y][(int)endingPos.x] = Room.WALL | Room.INDESTRUCTIBLE;
                                endingPos.x = px;
                                endingPos.y = py;
                                data.map[py][px] = Room.ENEMY | Room.DOOR | Room.ENDING;
                            } else {
                                paintCol = ILLEGAL;
                            }
                        } else {
                            if((palette.property & Room.PLAYER) == Room.PLAYER){
                                if((data.map[(int)data.player.y][(int)data.player.x] & Room.PLAYER) == Room.PLAYER){
                                    data.map[(int)data.player.y][(int)data.player.x] = Room.EMPTY;
                                }
                                data.player.x = px;
                                data.player.y = py;
                            }
                            if(palette.propertyLegal){
                                data.map[py][px] = palette.property;
                            } else {
                                paintCol = ILLEGAL;
                            }
                        }
                    } else {
                        paintCol = ILLEGAL;
                    }
                }
            }
		}
		
		public void setActive(Boolean value) {
            active = value;
            game.level.uiManager.setActive(!active);
            game.level.uiManager.mouseLock = true;
            game.level.uiManager.ignore = true;
            if(active){
                if(!palette.scrollButton.active){
                    renderer.camera.toggleDragScroll();
                }
                var list = new flash.Array<Point>();
                Room.setPropertyLocations(0, 0, data.width, data.height, data.map, Room.ENDING, list);
                endingPos = list[0] ?? endingPos;
            } else {
                if(renderer.camera.dragScroll){
                    renderer.camera.toggleDragScroll();
                }
                renderer.trackPlayer = true;
                if(game.level.room.type == Room.PUZZLE){
                    game.level.blackOutMap = game.level.data.getBlackOutMap();
                }
            }
		}
		
		public void render() {
            if(!palette.uiManager.mouseLock && palette.uiManager.currentGroup == 0){
                if(game.mousePressed){
                    if((palette.property & Room.PLAYER) == Room.PLAYER) {
                        if(game.mousePressedCount != game.frameCount) return;
                    }
                    var blit = renderer.paintBlit;
                    blit.x = (int)(renderer.canvasPoint.x + px * Game.SCALE);
                    blit.y = (int)(renderer.canvasPoint.y + py * Game.SCALE);
                    blit.render(renderer.bitmapData, paintCol);
                }
            }
		}
    }
}
