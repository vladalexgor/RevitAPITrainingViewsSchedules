using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Prism.Commands;
using RevitAPITrainingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPITrainingViewsSchedules
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;
        private Document _doc;

        public List<ViewPlan> Views { get; } = new List<ViewPlan>();
        public List<Category> Categories { get; } = new List<Category>();
        public DelegateCommand HideCommand { get; }
        public DelegateCommand TempHideCommand { get; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _doc = _commandData.Application.ActiveUIDocument.Document;
            Views = ViewsUtils.GetFloorPlanViews(_doc);
            Categories = CategoryUtils.GetCategories(_doc);
            HideCommand = new DelegateCommand(OnHideCommand);
            TempHideCommand = new DelegateCommand(OnTempHideCommand);
        }

        private void OnTempHideCommand()
        {
            if (SelectedViewPlan == null || SelectedCategory == null)
                return;
            using (var ts = new Transaction(_doc, "Save changes"))
            {
                ts.Start();
                SelectedViewPlan.HideCategoryTemporary(SelectedCategory.Id);
                ts.Commit();
            }

            RaiseCloseRequest();
        }

        public ViewPlan SelectedViewPlan { get; set; }

        public Category SelectedCategory { get; set; }

        private void OnHideCommand()
        {
            if (SelectedViewPlan == null || SelectedCategory == null)
                return;

            using (var ts = new Transaction(_doc, "Save changes"))
            {
                ts.Start();
                SelectedViewPlan.SetCategoryHidden(SelectedCategory.Id, hide: true);
                ts.Commit();
            }

            RaiseCloseRequest();
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
