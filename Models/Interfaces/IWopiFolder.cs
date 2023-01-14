namespace Wopi.Models.Interfaces
{
    public interface IWopiFolder
    {
        string Name { get; }

        /// <summary>
        /// Unique identifier of the folder.
        /// </summary>
        string Identifier { get; }
    }
}
