﻿using MALClient.Models.Enums.Enums;

namespace MALClient.Models.Enums
{
    public enum AnimeListWorkModes
    {
        Anime,
        SeasonalAnime,
        Manga,
        TopAnime,
        TopManga,
        AnimeByGenre,
        AnimeByStudio
    }

    public enum AnimeListDisplayModes
    {      
        IndefiniteList,
        IndefiniteGrid,
        IndefiniteCompactList,
    }

    public enum SortOptions
    {
        [EnumUtilities.Description("Title")]
        SortTitle,
        [EnumUtilities.Description("Score")]
        SortScore,
        [EnumUtilities.Description("Watched")]
        SortWatched,
        [EnumUtilities.Description("Air day")]
        SortAirDay,
        [EnumUtilities.Description("Last updated")]
        SortLastWatched,
        [EnumUtilities.Description("Start date")]
        SortStartDate,
        [EnumUtilities.Description("End Date")]
        SortEndDate,
        [EnumUtilities.Description("None")]
        SortNothing,
        [EnumUtilities.Description("Season")]
        SortSeason
    }
}