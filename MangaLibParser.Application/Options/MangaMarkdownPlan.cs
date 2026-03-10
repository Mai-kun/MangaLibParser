using System.Text;
using MangaLibParser.Domain.Entities;

namespace MangaLibParser.Application.Options;

public class MangaMarkdownPlan
{
    private readonly List<Action<StringBuilder, Manga>> _actions;

    public MangaMarkdownPlan(List<Action<StringBuilder, Manga>> actions)
    {
        _actions = actions;
    }

    public string Execute(Manga manga)
    {
        var sb = new StringBuilder();
        foreach (var action in _actions)
        {
            action(sb, manga);
        }

        return sb.ToString();
    }
}