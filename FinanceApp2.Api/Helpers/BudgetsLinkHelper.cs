using FinanceApp2.Api.Controllers;
using FinanceApp2.Shared.Models;

namespace FinanceApp2.Api.Helpers
{
    public class BudgetsLinkHelper
    {
        private readonly LinkGenerator _linkGenerator;

        public BudgetsLinkHelper(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        private string GetPath(string action, object? values = null)
        {
            return _linkGenerator.GetPathByAction(action, "Budgets", values)?.ToLower() ?? string.Empty;
        }

        public List<Link> GetLinksForBudgetsGet(int year, int month, bool isBudgetFound)
        {
            DateOnly requestedDate = new DateOnly(year, month, 1);
            DateOnly previousMonthDate = requestedDate.AddMonths(-1);
            DateOnly nextMonthDate = requestedDate.AddMonths(1);
            
            var links = new List<Link>()
            {
                BudgetsGetSelf(year, month)
            };

            if (isBudgetFound)
            {
                links.Add(BudgetsUpdate(year, month));
                links.Add(BudgetsDelete(year, month));
            }
            else
            {
                links.Add(BudgetsCreate());
            }

            links.Add(BudgetsGetPrevious(previousMonthDate.Year, previousMonthDate.Month));
            links.Add(BudgetsGetNext(nextMonthDate.Year, nextMonthDate.Month));

            return links;
        }

        public List<Link> GetLinksForBudgetsCreate(int year, int month)
        {
            DateOnly requestedDate = new DateOnly(year, month, 1);
            DateOnly previousMonthDate = requestedDate.AddMonths(-1);
            DateOnly nextMonthDate = requestedDate.AddMonths(1);

            return new List<Link>()
            {
                BudgetsGetSelf(year, month),
                BudgetsUpdate(year, month),
                BudgetsDelete(year, month),
                BudgetsGetPrevious(previousMonthDate.Year, previousMonthDate.Month),
                BudgetsGetNext(nextMonthDate.Year, nextMonthDate.Month)
            };
        }

        public Link BudgetsGetSelf(int year, int month)
        {
            return new Link
            {
                Href = GetPath(nameof(BudgetsController.Get), new { year = year, month = month }),
                Rel = "self",
                Method = "GET"
            };
        }

        public Link BudgetsCreate()
        {
            return new Link
            {
                Href = GetPath(nameof(BudgetsController.Create)),
                Rel = "create",
                Method = "POST"
            };
        }

        public Link BudgetsUpdate(int year, int month)
        {
            return new Link
            {
                Href = GetPath(nameof(BudgetsController.Update), new { year = year, month = month }),
                Rel = "update",
                Method = "PATCH"
            };
        }

        public Link BudgetsDelete(int year, int month)
        {
            return new Link
            {
                Href = GetPath(nameof(BudgetsController.Delete), new { year = year, month = month }),
                Rel = "delete",
                Method = "DELETE"
            };
        }

        public Link BudgetsGetPrevious(int year, int month)
        {
            return new Link
            {
                Href = GetPath(nameof(BudgetsController.Get), new { year = year, month = month }),
                Rel = "previous-month",
                Method = "GET"
            };
        }

        public Link BudgetsGetNext(int year, int month)
        {
            return new Link
            {
                Href = GetPath(nameof(BudgetsController.Get), new { year = year, month = month }),
                Rel = "next-month",
                Method = "GET"
            };
        }
    }
}
