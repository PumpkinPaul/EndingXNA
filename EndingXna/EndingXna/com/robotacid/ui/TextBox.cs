using System;
using System.Collections.Generic;
using com.robotacid.gfx;
using flash;
using flash.display;
using flash.geom;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Array = flash.Array;
using Level = com.robotacid.engine.Level;
using LevelData = com.robotacid.engine.LevelData;
using Room = com.robotacid.engine.Room;
using Renderer = com.robotacid.gfx.Renderer;
using BitmapData = flash.display.BitmapData;
using Point = flash.geom.Point;
using Rectangle = flash.geom.Rectangle;

namespace com.robotacid.ui
{
    /// <summary>
    /// Custom bitmap font - only renders one font between many instances
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class TextBox : Shape {

        public static BitmapData spriteSheet;
		public static Dictionary<char, Rectangle> characters;
		// the order in which to submit rects
		public static Array<char> characterNames = new Array<char> {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '\'', '\\', ':', ',', '=', '!', '/', '-', '(', '+', '?', ')', ';', '.', '@', '_', '%', '*', '\''};
		
		public BitmapData bitmapData;
		public Array<Array<Rectangle>> lines;						// a 2D array of all the bitmapDatas used, in lines
		public Array<double > lineWidths;				// the width of each line of text (used for alignment)
		public Array<Array<char?>> textLines;					// a 2D array of the characters used (used for fetching offset and kerning data)
		public int tracking;					// tracking: the spacing between letters
		public String align;					// align: whether the text is centered, left or right aligned
		public String alignVert;				// align_vert: vertical alignment of the text
		public int lineSpacing;					// line_spacing: distance between each line of copy
		public Boolean wordWrap;				// turns wordWrap on and off
		public uint backgroundCol;
		public uint borderCol;
		public double  backgroundAlpha;
		public int leading;
		public int offsetX;
		public int offsetY;

        protected uint _colorInt;				// the actual uint of the color being applied
		protected ColorTransform _color;		// a color transform object that is applied to the whole TextBox
		
		protected int whitespaceLength;			// the distance a whitespace takes up
		
		protected int _width;
		protected int _height;
		protected String _text;
		protected Rectangle borderRect;
		protected Rectangle boundsRect;
		protected Rectangle maskRect;

        public const int BORDER_ALLOWANCE = 0;

        public Point _position = new Point();

        public TextBox(double  _width, double  _height, uint backgroundCol = 0xFF111111, uint borderCol = 0xFF99999U) {
            this._width = (int)_width;
			this._height = (int)_height;
			this.backgroundCol = backgroundCol;
			this.borderCol = borderCol;
			align = "left";
			alignVert = "top";
			_colorInt = 0x00FFFFFF;
			wordWrap = true;
			tracking = 0;
			leading = 1;
			whitespaceLength = 2;
			lineSpacing = 8;
			_text = "";
			
			lines = new Array<Array<Rectangle>>();
			
			borderRect = new Rectangle(0, 0, _width, _height);
			boundsRect = new Rectangle(0, 0, _width, _height);
			maskRect = new Rectangle(0, 0, 1, 1);
			bitmapData = new BitmapData(Renderer.GameSpriteSheet, (int)_width, (int)_height, true, 0x0);
            drawBorder();
        }

        /* This must be called before any TextBox is created so sprites can be drawn */
		public static void init(Array<Rectangle> characterList, BitmapData _spriteSheet) {
            spriteSheet = _spriteSheet;
            characters = new Dictionary<char, Rectangle>();
            char characterName;
            Rectangle rect;
            for(int i = 0; i < characterList.length; i++){
                if(characterList[i] != null){
                    rect = characterList[i];
                } else {
                    rect = new Rectangle();
                }
                characterName = characterNames[i];
                characters[characterName] = rect;
            }
		}

        public String text {
            set {
			    _text = value;
			    updateText();
			    draw();
            }

            get { return _text; }
		}
		
        public uint color {
            get { return _colorInt; }
		    set {
			_colorInt = value;
			if(value == 0xFFFFFF) {
				_color = null;
			} else {
				_color = new ColorTransform(
					((value >> 16) % 256) / 255,
					((value >> 8) % 256) / 255,
					(value % 256) / 255
				);
			}

			if(_color != null) transform.colorTransform = _color;
            }
		}

        public void setSize(int width, int height) {
			_width = width;
			_height = height;
			borderRect = new Rectangle(1, 1, _width - 2, _height - 2);
			boundsRect = new Rectangle(2, 2, _width - 4, _height - 4);
			bitmapData = new BitmapData(Renderer.GameSpriteSheet, width, height, true, 0x0);
			updateText();
			draw();
		}

        /* Calculates an array of BitmapDatas needed to render the text */
		protected void updateText() {
		
            // we create an array called lines that holds references to all of the
            // bitmapDatas needed and structure it like the text
			
            // the lines property is public so it can be used to ticker text
            lines = new Array<Array<Rectangle>>();
            lineWidths = new Array<double >();
            textLines = new Array<Array<char?>>();
			
            var currentLine = new Array<Rectangle>();
            var currentTextLine = new Array<char?>();
            int wordBeginning = 0;
            int currentLineWidth = 0;
            int completeWordsWidth = 0;
            int wordWidth = 0;
            var newLine = new Array<Rectangle>();
            var newTextLine = new Array<char?>();
            char c;
			
            if(_text == null) _text = "";
			
            String upperCaseText = _text.ToUpper();
			
            for(int i = 0; i < upperCaseText.Length; i++){
				
                c = upperCaseText[i];
				
                // new line characters
                if(c == '\n' || c == '\r' || c == '|'){
                    lines.push(currentLine);
                    textLines.push(currentTextLine);
                    lineWidths.push(currentLineWidth);
                    currentLineWidth = 0;
                    completeWordsWidth = 0;
                    wordBeginning = 0;
                    wordWidth = 0;
                    currentLine = new Array<Rectangle>();
                    currentTextLine = new Array<char?>();
                    continue;
                }
				
                // push a character into the array
                if(characters[c] != null){
                    // check we're in the middle of a word - spaces are null
                    if(currentLine.length > 0 && currentLine[currentLine.length -1] != null){
                        currentLineWidth += tracking;
                        wordWidth += tracking;
                    }
                    wordWidth += (int)characters[c].width;
                    currentLineWidth += (int)characters[c].width;
                    currentLine.push(characters[c]);
                    currentTextLine.push(c);
				
                // the character is a SPACE or unrecognised and will be treated as a SPACE
                } else {
                    if(currentLine.length > 0 && currentLine[currentLine.length - 1] != null){
                        completeWordsWidth = currentLineWidth;
                    }
                    currentLineWidth += whitespaceLength;
                    currentLine.push(null);
                    currentTextLine.push(null);
                    wordBeginning = currentLine.length;
                    wordWidth = 0;
                }
				
                // if the length of the current line exceeds the width, we splice it into the next line
                // effecting word wrap
				
                if(currentLineWidth > _width - (BORDER_ALLOWANCE * 2) && wordWrap){
                    // in the case where the word is larger than the text field we take back the last character
                    // and jump to a new line with it
                    if(wordBeginning == 0 && currentLine[currentLine.length - 1] != null){
                        currentLineWidth -= tracking + (int)currentLine[currentLine.length - 1].width;
                        // now we take back the offending last character
                        var lastBitmapData = currentLine.pop();
                        var lastChar = currentTextLine.pop();
						
                        lines.push(currentLine);
                        textLines.push(currentTextLine);
                        lineWidths.push(currentLineWidth);
						
                        currentLineWidth = (int)lastBitmapData.width;
                        completeWordsWidth = 0;
                        wordBeginning = 0;
                        wordWidth = (int)lastBitmapData.width;
                        currentLine = new Array<Rectangle>();   
                        currentLine.push(lastBitmapData);     
                        currentTextLine = new Array<char?>();   
                        currentTextLine.push(lastChar);     
                        continue;
                    }
					
                    newLine = currentLine.splice(wordBeginning, (uint)(currentLine.length - wordBeginning));
                    newTextLine = currentTextLine.splice(wordBeginning, (uint)(currentTextLine.length - wordBeginning));
                    lines.push(currentLine);
                    textLines.push(currentTextLine);
                    lineWidths.push(completeWordsWidth);
                    completeWordsWidth = 0;
                    wordBeginning = 0;
                    currentLine = newLine;
                    currentTextLine = newTextLine;
                    currentLineWidth = wordWidth;
                }
            }
            // save the last line
            lines.push(currentLine);
            textLines.push(currentTextLine);
            lineWidths.push(currentLineWidth);
			
		}
		
		/* Render */
		public void draw() {
			
            //TODO
            drawBorder();
			
            int i, j;
            var point = new Point();
            int x;
            int y = BORDER_ALLOWANCE + offsetY;
            int alignX = 0;
            int alignY = 0;
            Rectangle charx;
            Point offset;
            int wordBeginning = 0;
            int linesHeight = lineSpacing * lines.length;
			
            for(i = 0; i < lines.length; i++, point.y += lineSpacing){
                x = BORDER_ALLOWANCE + offsetX;
				
                wordBeginning = 0;
                for(j = 0; j < lines[i].length; j++){
                    charx = lines[i][j];
					
                    // alignment to bitmap
                    if(align == "left"){
                        alignX = 0;
                    } else if(align == "center"){
                        alignX = (int)(_width * 0.5 - (lineWidths[i] * 0.5 + BORDER_ALLOWANCE));
                    } else if(align == "right"){
                        alignX = (int)(_width - lineWidths[i]);
                    }
                    if(alignVert == "top"){
                        alignY = 0;
                    } else if(alignVert == "center"){
                        alignY = (int)(_height * 0.5 - linesHeight * 0.5);
                    } else if(alignVert == "bottom"){
                        alignY = _height - linesHeight;
                    }
					
                    // print to bitmapdata
                    if(charx != null){
                        if(j > wordBeginning){
                            x += tracking;
                        }
                        point.x = alignX + x;
                        point.y = alignY + y + leading;
                        // mask characters that are outside the boundsRect
                        if(
                            point.x < boundsRect.x ||
                            point.y < boundsRect.y ||
                            point.x + charx.width >= boundsRect.x + boundsRect.width ||
                            point.y + charx.height >= boundsRect.y + boundsRect.height
                        ){
                            // are they even in the bounds rect?
                            if(
                                point.x + charx.width > boundsRect.x &&
                                boundsRect.x + boundsRect.width > point.x &&
                                point.y + charx.height > boundsRect.y &&
                                boundsRect.y + boundsRect.height > point.y
                            ){
                                // going to make a glib assumption that the TextBox won't be smaller than a single character
                                maskRect.x = point.x >= boundsRect.x ? charx.x : charx.x + (point.x - boundsRect.x);
                                maskRect.y = point.y >= boundsRect.y ? charx.y : charx.y + (point.y - boundsRect.y);
                                // NB: just changed this class over to a sprite sheet, no idea if the above lines actually work
                                maskRect.width = point.x + charx.width <= boundsRect.x + boundsRect.width ? charx.width : (boundsRect.x + boundsRect.width) - point.x;
                                maskRect.height = point.y + charx.height <= boundsRect.y + boundsRect.height ? charx.height : (boundsRect.y + boundsRect.height) - point.y;
                                if(point.x < boundsRect.x){
                                    maskRect.x = boundsRect.x - point.x;
                                    point.x = boundsRect.x;
                                }
                                if(point.y < boundsRect.y){
                                    maskRect.y = boundsRect.y - point.y;
                                    point.y = boundsRect.y;
                                }
                                //bitmapData.copyPixels(spriteSheet, maskRect, point, null, null, true);
                            }
                        } else {
                            //bitmapData.copyPixels(spriteSheet, charx, point, null, null, true);
                        }
                        x += (int)charx.width;
                    } else {
                        x += whitespaceLength;
                        wordBeginning = j + 1;
                    }
                }
                y += lineSpacing;

                _position.x = alignX;
                _position.y = alignY;
            }
			
            if(_color != null) transform.colorTransform = _color;
			
            //TODO
            //graphics.clear();
            //graphics.lineStyle(0, 0, 0);
            //graphics.beginBitmapFill(bitmapData);
            //graphics.drawRect(0, 0, _width, _height);
            //graphics.endFill();
		}
		
		public void drawBorder() {
            //TODO: put back
			//bitmapData.fillRect(bitmapData.rect, borderCol);
			//bitmapData.fillRect(borderRect, backgroundCol);
		}
		
		public void renderTo(double  x, double  y, BitmapData target) {
            var p = new Point(x, y);
            target.copyPixels(bitmapData, bitmapData.rect, p, null, null, true);
		}

        //TODO - remove
        protected internal override void OnDraw(RenderTarget2D sceneRenderTarget, GameTime gameTime) {
            XnaGame.Instance.FlashRenderer.FillRect(null, bitmapData.rect, backgroundCol, (float)effectiveAlpha);   
            XnaGame.Instance.FlashRenderer.DrawText(null, text, bitmapData.rect, _position, _colorInt, (float)effectiveAlpha);  
        }
    }
}
