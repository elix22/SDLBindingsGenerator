


/*
elix22 - notes

Some demo utilizing CppSharp , generating automated  C# bindings for SDL .
This App will first try to clone the latest SDL repo and then will try to generate the C# bindings in the Bindings folder.
Verified on Windows , should work on Linux and Mac
*/

using CppSharp;
using CppSharp.AST;
using CppSharp.Passes;
using CppSharp.Generators;
using LibGit2Sharp;

namespace SDLBindingsGenerator
{
    class SDL : ILibrary
    {

        string outputDirectory = "Bindings";

        public void Setup(Driver driver)
        {
            var options = driver.Options;
            var parserOptions = driver.ParserOptions;

            options.GeneratorKind = GeneratorKind.CSharp;
            options.GenerationOutputMode = GenerationOutputMode.FilePerUnit;
            options.OutputDir = outputDirectory;
           
            // options.Verbose = true;

            var module = options.AddModule("SDL");
            module.Headers.Add("SDL.h");


            //parserOptions.Verbose = true;
            // parserOptions.EnableRTTI = true;
            var sdlPath = Path.Combine(Directory.GetCurrentDirectory(), "SDL/include");
            parserOptions.AddIncludeDirs(sdlPath);
            module.IncludeDirs.Add(sdlPath);
        }

        public void SetupPasses(Driver driver)
        {
            // driver.Context.TranslationUnitPasses.RemovePrefix("SDL_");
            driver.Context.TranslationUnitPasses.RemovePrefix("SCANCODE_");
            driver.Context.TranslationUnitPasses.RemovePrefix("SDLK_");
            driver.Context.TranslationUnitPasses.RemovePrefix("KMOD_");
            driver.Context.TranslationUnitPasses.RemovePrefix("LOG_CATEGORY_");
        }

        public void Preprocess(Driver driver, ASTContext ctx)
        {

            // ctx.IgnoreEnumWithMatchingItem("SDL_FALSE");
            ctx.IgnoreEnumWithMatchingItem("DUMMY_ENUM_VALUE");

            ctx.SetNameOfEnumWithMatchingItem("SDL_SCANCODE_UNKNOWN", "ScanCode");
            ctx.SetNameOfEnumWithMatchingItem("SDLK_UNKNOWN", "Key");
            ctx.SetNameOfEnumWithMatchingItem("KMOD_NONE", "KeyModifier");
            ctx.SetNameOfEnumWithMatchingItem("SDL_LOG_CATEGORY_CUSTOM", "LogCategory");

            ctx.GenerateEnumFromMacros("InitFlags", "SDL_INIT_(.*)").SetFlags();
            ctx.GenerateEnumFromMacros("Endianness", "SDL_(.*)_ENDIAN");
            ctx.GenerateEnumFromMacros("InputState", "SDL_RELEASED", "SDL_PRESSED");
            ctx.GenerateEnumFromMacros("AlphaState", "SDL_ALPHA_(.*)");
            ctx.GenerateEnumFromMacros("HatState", "SDL_HAT_(.*)");

            ctx.IgnoreHeadersWithName("SDL_atomic*");
            ctx.IgnoreHeadersWithName("SDL_endian*");
            ctx.IgnoreHeadersWithName("SDL_main*");
            ctx.IgnoreHeadersWithName("SDL_mutex*");
            ctx.IgnoreHeadersWithName("SDL_stdinc*");
            ctx.IgnoreHeadersWithName("SDL_error");

            ctx.IgnoreEnumWithMatchingItem("SDL_ENOMEM");
            ctx.IgnoreFunctionWithName("SDL_Error");

        }

        public void Postprocess(Driver driver, ASTContext ctx)
        {
            ctx.SetNameOfEnumWithName("PIXELTYPE", "PixelType");
            ctx.SetNameOfEnumWithName("BITMAPORDER", "BitmapOrder");
            ctx.SetNameOfEnumWithName("PACKEDORDER", "PackedOrder");
            ctx.SetNameOfEnumWithName("ARRAYORDER", "ArrayOrder");
            ctx.SetNameOfEnumWithName("PACKEDLAYOUT", "PackedLayout");
            ctx.SetNameOfEnumWithName("PIXELFORMAT", "PixelFormats");
            ctx.SetNameOfEnumWithName("assert_state", "AssertState");
            ctx.SetClassBindName("assert_data", "AssertData");
            ctx.SetNameOfEnumWithName("eventaction", "EventAction");
            ctx.SetNameOfEnumWithName("LOG_CATEGORY", "LogCategory");

            // TBD elix22 , some hack , should find the proper way.
            string path = Path.Combine(Directory.GetCurrentDirectory(), outputDirectory, "SDL_bool_enum.cs");
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("namespace SDL { public enum SDL_bool{SDL_FALSE = 0,SDL_TRUE = 1}; }");
                }
            }
      
        }


        static class Program
        {
            public static void Main(string[] args)
            {
                CloneSDLRepo();

                ConsoleDriver.Run(new SDL());
            }

            static void CloneSDLRepo()
            {
                string sdlDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SDL");

                if (Directory.Exists(sdlDirectory))
                {
                    string logMessage = "";
                    using (var repo = new Repository(sdlDirectory))
                    {
                        Console.WriteLine("fetching SDL Repository from https://github.com/spurious/SDL-mirror.git");
                        var remote = repo.Network.Remotes["origin"];
                        var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                        Commands.Fetch(repo, remote.Name, refSpecs, null, logMessage);
                        Console.WriteLine(logMessage);
                    }
                }
                else
                {
                    Console.WriteLine("Cloning SDL Repository from https://github.com/spurious/SDL-mirror.git");
                    string path = Repository.Clone("https://github.com/spurious/SDL-mirror.git", sdlDirectory);
                }


            }
        }
    }
}
