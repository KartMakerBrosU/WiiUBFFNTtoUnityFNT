# WiiUBFFNTtoUnityFNT
 Converts Wii U's BFFNT to Unity's Bitmap Font format.
 
## How to Use
1. Download the release
2. Run the file and select your BFFNT File
   - The converted .fontsettings file will be alongside the BFFNT File.
3. Drag the .fontsettings file into your Unity Project
4. Extract the texture from the BFFNT file using a method from your choosing and add it alongside the .fontsettings file.
5. Create a material in Unity and set its shader to `GUI/Text Shader` (You can also make your own)
6. Select the font in Unity and drag the Material into the `Default Material` slot.
7. Done!
You can now use this font anywhere in your project.

## Notice
The FontSettings file will only display the Main Texture of the material without shader effects.
If you want the shader to modify the text, add the Material into the `Material` slot of the Legacy Text GameObject

## Credits
- kinnay for Nintendo-File-Format site used for Documentation.
- KillZXGaming for additional Documentation within Toolbox.
- Snowberry Software for helpful Binary IO Libraries and extentions.

## TODO
- Handle multiple texture sheets (Might have to combine them extenally)
- Handle multiple character maps. (Implemented but untested with current resources)
