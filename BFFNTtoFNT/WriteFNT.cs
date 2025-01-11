using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Core;
using Xunit.Abstractions;
using System.Diagnostics;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Core.Events;
using System.Xml.Linq;
using BFFNTtoFNT.Classes;
using System.Collections;
using Snowberry.IO.Writer;

namespace BFFNTtoFNT
{
    public class WriteFNT
    {
        FontSettings Font;
        public WriteFNT(FontSettings fnt,string fntSettDir) {
            Font = fnt;
            WriteFntSettingsFile(fntSettDir); 
        }

        //Writes the Unity .fontsettings file.
        public void WriteFntSettingsFile(string fileWriteDir)
        {
            
            var serializer = new SerializerBuilder().WithEventEmitter(next => new MixedStyleEmitter(next, Font)).Build();
            string yaml = serializer.Serialize(Font);
            yaml = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!128 &12800000\n" + yaml;
            EndianStreamWriter writer = new EndianStreamWriter(File.OpenWrite(fileWriteDir),false);
            writer.WriteLine(yaml);
            writer.Close();
            Application.Exit();
        }

    }
    
    //MixedStyleEmitter that allows for inline and multiline dictionaries/arrays/lists to be used. 
    public class MixedStyleEmitter : ChainedEventEmitter
    {
        private readonly FontSettings _data;

        public MixedStyleEmitter(IEventEmitter nextEmitter, FontSettings data) : base(nextEmitter)
        {
            _data = data;
        }

        public override void Emit(MappingStartEventInfo eventInfo, IEmitter emitter)
        {
            // Check if the source is a dictionary
            if (eventInfo.Source.Value is IDictionary dictionary)
            {
                // Use inline style for smaller dictionaries (e.g., InlineValues)
                if (dictionary.Count <= 3) // Example condition: inline if dictionary has 3 or fewer entries
                {
                    eventInfo = new MappingStartEventInfo(eventInfo.Source)
                    {
                        Style = MappingStyle.Flow // Inline style
                    };
                }
                else
                {
                    // Use block style for larger dictionaries
                    eventInfo = new MappingStartEventInfo(eventInfo.Source)
                    {
                        Style = MappingStyle.Block // Block style
                    };
                }
            }
            if(eventInfo.Source.Value is Array array && array.Length == 0)
            {
                emitter.Emit(new Scalar(null, null, "[]", ScalarStyle.Plain, true, false));
            }
            base.Emit(eventInfo, emitter);
        }
    }
}
