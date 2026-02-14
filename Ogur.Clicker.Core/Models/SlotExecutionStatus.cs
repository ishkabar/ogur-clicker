// Ogur.Clicker.Core/Models/SlotExecutionStatus.cs
namespace Ogur.Clicker.Core.Models;

public enum SlotExecutionStatus
{
    NoFocus,    // Szare - gra nie ma focusu
    Ready,      // Zielone - gotowe
    Executing   // Czerwone - w trakcie klikania
}