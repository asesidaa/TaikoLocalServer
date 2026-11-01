namespace TaikoWebUI.Components.Ui;

/// <summary>Shared Tailwind class strings, so every page's controls and cards look the same.</summary>
public static class Css
{
    public const string Card = "rounded-2xl border border-zinc-200/80 bg-white shadow-[0_1px_2px_rgba(0,0,0,.04)] dark:border-zinc-800 dark:bg-zinc-900";

    public const string Input = "w-full rounded-xl border border-zinc-200 bg-zinc-50 px-3 py-2.5 text-sm outline-none transition-[border-color,box-shadow] placeholder:text-zinc-400 focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20 disabled:cursor-not-allowed disabled:opacity-50 dark:border-zinc-700 dark:bg-zinc-800/60 dark:placeholder:text-zinc-500";

    public const string PrimaryButton = "inline-flex items-center justify-center gap-1.5 rounded-xl bg-primary-500 px-5 py-2.5 text-sm font-medium text-white transition-[background-color,transform] hover:bg-primary-600 active:scale-[.98] disabled:cursor-not-allowed disabled:opacity-50";

    public const string GhostButton = "inline-flex items-center justify-center gap-1.5 rounded-xl px-4 py-2.5 text-sm font-medium text-zinc-600 hover:bg-zinc-100 dark:text-zinc-300 dark:hover:bg-zinc-800";

    public const string IconButton = "grid size-9 place-items-center rounded-xl text-zinc-600 transition-colors hover:bg-zinc-100 hover:text-zinc-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-zinc-100";

    /// <summary>A toggle chip's state (genre and filter chips): dark when on, grey when off.</summary>
    public static string Chip(bool on) => on
        ? "bg-zinc-900 text-white dark:bg-white dark:text-zinc-900"
        : "bg-zinc-100 text-zinc-600 hover:bg-zinc-200 dark:bg-zinc-800 dark:text-zinc-300 dark:hover:bg-zinc-700";

    /// <summary>A plain filter pill's state (segmented filters): dark when on, no background when off.</summary>
    public static string Pill(bool on) => on
        ? "bg-zinc-900 text-white dark:bg-white dark:text-zinc-900"
        : "text-zinc-600 hover:bg-zinc-100 dark:text-zinc-300 dark:hover:bg-zinc-800";

    /// <summary>A chip in a phone Rail that stands in for a side list (Game data files, Dani Dojo sets).</summary>
    public static string RailChip(bool on) => "flex shrink-0 snap-start items-center gap-1.5 whitespace-nowrap rounded-full px-3 py-1.5 text-sm font-medium " + (on
        ? "bg-zinc-900 text-white dark:bg-white dark:text-zinc-900"
        : "bg-white text-zinc-600 ring-1 ring-zinc-200 dark:bg-zinc-900 dark:text-zinc-300 dark:ring-zinc-700");

    /// <summary>An item in an ActionMenu (role="menuitem"); add text-red-600 dark:text-red-400 for a destructive one.</summary>
    public const string MenuItem = "flex w-full items-center gap-2.5 whitespace-nowrap rounded-lg px-3 py-2 text-left text-sm hover:bg-zinc-100 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-transparent dark:hover:bg-zinc-800";

    public const string Muted = "text-zinc-500 dark:text-zinc-400";

    public const string Link = "font-medium text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300";

    /// <summary>ARIA state value. Blazor renders a bool attribute as present/absent, but aria-* need "true"/"false".</summary>
    public static string Aria(bool on) => on ? "true" : "false";
}
