﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using MALClient.Models.Models.ApiResponses;
using MALClient.XShared.Comm.Profile;
using MALClient.XShared.NavArgs;
using MALClient.XShared.Utils.Enums;

namespace MALClient.XShared.ViewModels.Main
{
    public class HummingbirdProfilePageViewModel : ViewModelBase
    {
        private bool _loaded;
        public HumProfileData CurrentData { get; set; } = new HumProfileData();
        public List<HumStoryObject> FeedData { get; set; } = new List<HumStoryObject>();
        public List<HumStoryObject> SocialFeedData { get; set; } = new List<HumStoryObject>();

        public ObservableCollection<AnimeItemViewModel> FavAnime { get; } =
            new ObservableCollection<AnimeItemViewModel>();

        public AnimeItemViewModel TemporarilySelectedAnimeItem
        {
            get { return null; }
            set
            {
                value?.NavigateDetails(PageIndex.PageProfile,
                    new ProfilePageNavigationArgs());
            }
        }

        public async void Init(bool force = false)
        {
            try
            {
                if (!_loaded || force)
                {
                    FavAnime.Clear();
                    CurrentData = await new ProfileQuery().GetHumProfileData();
                    foreach (var fav in CurrentData.favorites)
                    {
                        var data = await ViewModelLocator.AnimeList.TryRetrieveAuthenticatedAnimeItem(fav.item_id);
                        if (data != null)
                        {
                            FavAnime.Add(data as AnimeItemViewModel);
                        }
                    }
                    RaisePropertyChanged(() => CurrentData);
                    var feed = await new ProfileQuery(true).GetHumFeedData();
                    foreach (var entry in feed)
                        entry.substories = entry.substories.Take(8).ToList();
                    SocialFeedData = FeedData.Where(o => o.story_type == "comment").ToList();
                    FeedData = FeedData.Where(o => o.story_type == "media_story").ToList();

                    RaisePropertyChanged(() => FeedData);
                }
            }
            catch (Exception)
            {                
                //#justhummingbirdthings
            }
            
        }
    }
}