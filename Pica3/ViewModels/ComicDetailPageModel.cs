﻿using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Pica3.CoreApi;
using Pica3.CoreApi.Comic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pica3.ViewModels;

public sealed partial class ComicDetailPageModel : ObservableObject, IViewModel
{


    private readonly PicaClient picaClient;


    public string ComicId { get; private set; }


    private string coverPlaceholderUrl;


    public ComicDetailPageModel(PicaClient picaClient)
    {
        this.picaClient = picaClient;
    }



    public void Initialize(object? param = null)
    {
        if (param is ComicProfile comic)
        {
            ComicId = comic.Id;
            coverPlaceholderUrl = comic.Cover.Url;
        }
    }



    public async void Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Uri.TryCreate(coverPlaceholderUrl, UriKind.RelativeOrAbsolute, out var uri))
            {
                var file = await PicaFileCache.Instance.GetFileFromCacheAsync(uri);
                if (file != null)
                {
                    CoverPlaceholder = new BitmapImage(new Uri(file.Path));
                }
            }
            if (ComicDetailInfo is null)
            {
                ComicDetailInfo = await picaClient.GetComicDetailAsync(ComicId);
                IsLiked = ComicDetailInfo.IsLiked;
                IsFavourite = ComicDetailInfo.IsFavourite;
                var pageResult = await picaClient.GetComicEpisodeAsync(ComicId, 1);
                EpisodeProfiles = new(pageResult.TList);
                TotalEpisodePage = pageResult.Pages;
                CurrentEpisodePage = pageResult.Page;
            }
            RecommendComics ??= await picaClient.GetRecommendComicsAsync(ComicId);
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
    }



    /// <summary>
    /// 封面
    /// </summary>
    [ObservableProperty]
    private BitmapImage coverPlaceholder;


    /// <summary>
    /// 漫画详情
    /// </summary>
    [ObservableProperty]
    private ComicDetail comicDetailInfo;


    /// <summary>
    /// 推荐
    /// </summary>
    [ObservableProperty]
    private List<ComicProfile> recommendComics;


    public ComicProfile? LastClickedComic { get; set; }






    #region 点赞收藏


    /// <summary>
    /// 点赞
    /// </summary>
    [ObservableProperty]
    private bool isLiked;

    /// <summary>
    /// 收藏
    /// </summary>
    [ObservableProperty]
    private bool isFavourite;


    /// <summary>
    /// 给漫画点赞
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task LikeComicAsync()
    {
        try
        {
            if (ComicDetailInfo != null)
            {
                if (await picaClient.LikeComicAsync(ComicId))
                {
                    IsLiked = true;
                }
                else
                {
                    IsLiked = false;
                }
            }
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
    }



    /// <summary>
    /// 收藏漫画
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task StarComicAsync()
    {
        try
        {
            if (ComicDetailInfo != null)
            {
                if (await picaClient.AddFavoriteAsync(ComicId))
                {
                    IsFavourite = true;
                }
                else
                {
                    IsFavourite = false;
                }
            }
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
    }


    #endregion




    #region 章节



    /// <summary>
    /// 漫画章节
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ComicEpisodeProfile> episodeProfiles;



    /// <summary>
    /// 全部章节页数
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NextPageVisibility))]
    private int totalEpisodePage;

    /// <summary>
    /// 已加载章节页数
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NextPageVisibility))]
    private int currentEpisodePage;

    /// <summary>
    /// 下一页按键是否可见
    /// </summary>
    public Visibility NextPageVisibility => CurrentEpisodePage < TotalEpisodePage ? Visibility.Visible : Visibility.Collapsed;



    /// <summary>
    /// 加载下一页章节
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task LoadNextEpisodePageAsync()
    {
        try
        {
            if (TotalEpisodePage == 0)
            {
                var countResult = await picaClient.GetComicEpisodeAsync(ComicId, 1);
                EpisodeProfiles = new(countResult.TList);
                TotalEpisodePage = countResult.Pages;
                CurrentEpisodePage = countResult.Page;
            }
            else
            {
                if (CurrentEpisodePage < TotalEpisodePage)
                {
                    var countResult = await picaClient.GetComicEpisodeAsync(ComicId, CurrentEpisodePage + 1);
                    countResult.TList.ForEach(x => EpisodeProfiles?.Add(x));
                    CurrentEpisodePage = countResult.Page;
                }
            }
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
    }



    /// <summary>
    /// 加载全部章节
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task LoadAllEpisodePageAsync()
    {
        try
        {
            if (TotalEpisodePage == 0)
            {
                var countResult = await picaClient.GetComicEpisodeAsync(ComicId, 1);
                EpisodeProfiles = new(countResult.TList);
                TotalEpisodePage = countResult.Pages;
                CurrentEpisodePage = countResult.Page;
            }
            while (CurrentEpisodePage < TotalEpisodePage)
            {
                var countResult = await picaClient.GetComicEpisodeAsync(ComicId, CurrentEpisodePage + 1);
                countResult.TList.ForEach(x => EpisodeProfiles?.Add(x));
                CurrentEpisodePage = countResult.Page;
            }
        }
        catch (Exception ex)
        {
            ex.HandlePicaException();
        }
    }


    #endregion






}
