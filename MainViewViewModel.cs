using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Prism.Commands;
using RevitAPITrainingLibrary;
using RevitAPITrainingViewsSchedules.Wrappers;
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

        public List<ScheduleWrapper> Schedules { get; }
        public DelegateCommand AddFilterCommand { get; }
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _doc = _commandData.Application.ActiveUIDocument.Document;
            Schedules = ScheduleUtils.GetAllTheSchedules(_doc).Select(s => new ScheduleWrapper(s)).ToList();
            AddFilterCommand = new DelegateCommand(OnAddFilterCommand);
        }

        private void OnAddFilterCommand()
        {
            List<ScheduleWrapper> selectedSchedules = Schedules.Where(s => s.IsSelected).ToList();
            if (selectedSchedules == null)
                return;

            using (var ts = new Transaction(_doc, "Add filter to schedule"))
            {
                ts.Start();
                foreach (var schedule in selectedSchedules)
                {
                    var scheduleDef = schedule.ViewSchedule.Definition;
                    if (scheduleDef == null)
                        continue;

                    ScheduleField field = FindField(schedule.ViewSchedule, ParameterName);
                    if (field == null)
                    {
                        SchedulableField schedulableField = FindSchedulableField(schedule.ViewSchedule, ParameterName);
                        if (schedulableField == null)
                            continue;

                        field = scheduleDef.AddField(schedulableField);
                    }

                    if (field == null)
                        continue;

                    var filter = new ScheduleFilter(field.FieldId, ScheduleFilterType.Equal, ParameterValue);
                    if (filter == null)
                        continue;

                    scheduleDef.AddFilter(filter);
                }
                ts.Commit();

                RaiseCloseRequest();
            }
        }

        private SchedulableField FindSchedulableField(ViewSchedule viewSchedule, string parameterName)
        {
            var schedulableField = viewSchedule.Definition.GetSchedulableFields()
                                        .FirstOrDefault(p => p.GetName(viewSchedule.Document) == parameterName);
            return schedulableField;
        }

        private ScheduleField FindField(ViewSchedule viewSchedule, string parameterName)
        {
            ScheduleDefinition definition = viewSchedule.Definition;
            var fieldCount = definition.GetFieldCount();

            for (int i = 0; i < fieldCount; i++)
            {
                var field = definition.GetField(i);
                var fieldName = field.GetName();

                if (fieldName == parameterName)
                    return definition.GetField(i);
            }

            return null;
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
