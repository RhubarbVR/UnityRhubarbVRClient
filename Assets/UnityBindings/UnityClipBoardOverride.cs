using System.Threading;
using System.Threading.Tasks;
using TextCopy;
using UnityEngine;

public class UnityClipBoardOverride : IClipboard
{
    public string GetText()
    {
        return GUIUtility.systemCopyBuffer;
    }

    public Task<string> GetTextAsync(CancellationToken cancellation = default)
    {
        return Task.Run(() =>
        {
            return GUIUtility.systemCopyBuffer;
        });
    }

    public void SetText(string text)
    {
        GUIUtility.systemCopyBuffer = text;
    }

    public Task SetTextAsync(string text, CancellationToken cancellation = default)
    {
        return Task.Run(() =>
        {
            GUIUtility.systemCopyBuffer = text;
        });
    }
}