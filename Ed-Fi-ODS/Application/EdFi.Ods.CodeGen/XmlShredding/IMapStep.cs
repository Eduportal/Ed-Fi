namespace EdFi.Ods.CodeGen.XmlShredding
{
    public interface IMapStep 
    {
        string[] GetXPath();
        IMapStep GetNextStep();
        bool IsTerminal();
    }
}