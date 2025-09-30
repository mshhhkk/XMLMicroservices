using Shared.Contracts;

namespace FileParserService.App.Ports;
public interface IStateMutator
{
    ModuleDTO Mutate(ModuleDTO module);
}
