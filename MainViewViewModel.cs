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

        public List<ViewPlan> Views { get; }
        public List<ParameterFilterElement> Filters { get; }
        public DelegateCommand AddFilterCommand { get; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _doc = _commandData.Application.ActiveUIDocument.Document;
            Views = ViewsUtils.GetFloorPlanViews(_doc);
            Filters = FilterUtils.GetFilters(_doc);
            AddFilterCommand = new DelegateCommand(OnAddFilterCommand);
        }

        public ParameterFilterElement SelectedFilter { get; set; }

        private void OnAddFilterCommand()
        {
            if (SelectedViewPlan == null || SelectedFilter == null)
                return;
            using (var ts = new Transaction(_doc, "Set filter"))
            {
                ts.Start();
                SelectedViewPlan.AddFilter(SelectedFilter.Id);
                OverrideGraphicSettings overrideGraphicSettings = SelectedViewPlan.GetFilterOverrides(SelectedFilter.Id);
                overrideGraphicSettings.SetProjectionLineColor(new Color(255, 0, 0));
                SelectedViewPlan.SetFilterOverrides(SelectedFilter.Id, overrideGraphicSettings);
                ts.Commit();
            }

            RaiseCloseRequest();
        }

        public ViewPlan SelectedViewPlan { get; set; }  

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
