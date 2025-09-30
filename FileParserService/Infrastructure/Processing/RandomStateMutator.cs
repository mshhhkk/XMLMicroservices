#nullable enable
using FileParserService.App.Ports;
using Shared.Contracts;

namespace FileParserService.Infrastructure.Processing;

public class RandomStateMutator : IStateMutator
{
    private static readonly ModuleState[] States =
        { ModuleState.Online, ModuleState.Run, ModuleState.NotReady, ModuleState.Offline };

    public ModuleDTO Mutate(ModuleDTO module)
        => module with { ModuleState = States[Random.Shared.Next(States.Length)] };
}
