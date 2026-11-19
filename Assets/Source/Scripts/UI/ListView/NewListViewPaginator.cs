using UIWidgets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

/// <summary>
/// ListView's paginator. Also works with TileView's.
/// </summary>
public class NewListViewPaginator : ListViewPaginator
{
    protected override void UpdatePageButtons(int currentPage)
    {
        if (SRDefaultPage == null)
        {
            return;
        }
        if (currentPage >= Pages)
        {
            currentPage = Pages - 1;
        }

        var visiblePages = new List<int>();
        for (int p = 0; p < Pages; p++) visiblePages.Add(p);
        var pagesToShow = visiblePages;

        if (MaxButtonsCount != 0 && Pages > MaxButtonsCount)
        {

            // essentially, we'd like the current button to be as close to the center as possible
            var desiredCurrentPageIndex = (int)Math.Floor(MaxButtonsCount / 2f);
            // the current page is always visible
            var tempPages = new List<int>() { currentPage };
            var lastStartIndexAdded = 0;

            if (currentPage > 0)
            {
                // add as many pages before the current until the current page is approximately halfway
                int maxIterations = desiredCurrentPageIndex;
                for (int x = currentPage - 1; x <= currentPage && x >= 0; x--)
                {
                    tempPages.Insert(0, visiblePages[x]);

                    lastStartIndexAdded = x;

                    maxIterations--;

                    if (maxIterations <= 0) break;
                }
            }

            var buttonsToAdd = MaxButtonsCount - tempPages.Count;

            // add pages at the end until we reach the desired amount, or run out of pages
            for (int x = 1; x <= buttonsToAdd; x++)
            {
                var index = currentPage + x;
                if (index >= Pages) break;
                tempPages.Add(visiblePages[currentPage + x]);
            }

            int currentIndex = lastStartIndexAdded - 1;
            while (currentIndex > 0 && tempPages.Count < MaxButtonsCount)
            {
                // if this executes, then we ran out of buttons at the end, so we need to add some more at the beginning
                tempPages.Insert(0, visiblePages[currentIndex]);
                currentIndex--;
            }

            pagesToShow = tempPages;
        }
        if (pagesToShow != DefaultPages.Keys.ToList())
        {
            foreach (var item in DefaultPages)
            {
                item.Value.gameObject.SetActive(false);
                DefaultPagesCache.Add(item.Value);
            }

            DefaultPages.Clear();
            for (var x = 0; x < pagesToShow.Count; x++)
            {
                var pageNumber = pagesToShow[x];
                AddComponent(pageNumber);
            }
        }


        var result = DefaultPages.TryGetValue(currentPage, out var page);
        if (result)
        {
            page.gameObject.SetActive(false);
            DefaultPagesCache.Add(page);
            DefaultPages.Remove(currentPage);
        }

        if (SRPrevPage != null) SRPrevPage.transform.SetAsFirstSibling();
        if (SRFirstPage != null) SRFirstPage.transform.SetAsFirstSibling();
        if (SRNextPage != null) SRNextPage.transform.SetAsLastSibling();
        if (SRLastPage != null) SRLastPage.transform.SetAsLastSibling();

        if (SRPrevPage != null) SRPrevPage.enabled = IsPrevAvailable(currentPage);
        if (SRNextPage != null) SRNextPage.enabled = IsNextAvailable(currentPage);

        LayoutUtilities.UpdateLayout(DefaultPage.parent.GetComponent<LayoutGroup>());
    }
    protected override void UpdateObjects(int page)
    {
        if (page >= Pages)
        {
            page = Pages - 1;
        }
        if (SRDefaultPage != null)
        {
            foreach (var item in DefaultPages)
            {
                if (item.Key == page)
                {
                    item.Value.gameObject.SetActive(false);
                    DefaultPagesCache.Add(item.Value);
                }
                else
                {
                    item.Value.gameObject.SetActive(true);
                }
            }
        }
        if (SRPrevPage != null) SRPrevPage.transform.SetAsFirstSibling();
        if (SRFirstPage != null) SRFirstPage.transform.SetAsFirstSibling();
        if (SRNextPage != null) SRNextPage.transform.SetAsLastSibling();
        if (SRLastPage != null) SRLastPage.transform.SetAsLastSibling();

        if (SRPrevPage != null) SRPrevPage.enabled = IsPrevAvailable(page);
        if (SRNextPage != null) SRNextPage.enabled = IsNextAvailable(page);
    }
}