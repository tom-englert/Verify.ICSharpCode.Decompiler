namespace VerifyTests;

public static partial class VerifyICSharpCodeDecompiler
{
    static readonly Regex RvaScrubber = new(@"[ \t]+// Method begins at RVA 0x[0-9A-F]+\r?\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool Initialized { get; private set; }

    [Obsolete("Use Initialize()")]
    public static void Enable() =>
        Initialize();

    public static void Initialize()
    {
        if (Initialized)
        {
            throw new("Already Initialized");
        }

        Initialized = true;

        VerifierSettings.RegisterFileConverter<TypeToDisassemble>(ConvertTypeDefinitionHandle);
        VerifierSettings.RegisterFileConverter<MethodToDisassemble>(ConvertMethodDefinitionHandle);
        VerifierSettings.RegisterFileConverter<PropertyToDisassemble>(ConvertPropertyDefinitionHandle);
        VerifierSettings.RegisterFileConverter<AssemblyToDisassemble>(ConvertAssembly);
        VerifierSettings.RegisterFileConverter<PEFile>(ConvertAssembly);
    }

    static ConversionResult Convert(IReadOnlyDictionary<string, object> context, Action<ReflectionDisassemblerImport> action) =>
        Convert(context, (disassembler, _) => action(disassembler));

    static ConversionResult Convert(IReadOnlyDictionary<string, object> context, Action<ReflectionDisassemblerImport, ITextOutput> action)
    {
        var output = new PlainTextOutput();
        var disassembler = new ReflectionDisassemblerImport(output, default);

        if (context.GetNormalizeIl())
        {
            disassembler.EntityProcessor = new SortByNameProcessor();
        }

        action(disassembler, output);

        var data = RvaScrubber.Replace(output.ToString(), string.Empty);

        return new(null, "txt", data);
    }
}