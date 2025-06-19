using CommunityToolkit.Mvvm.Input;
using MyLoginApp.Models;

namespace MyLoginApp.PageModels;

public interface IProjectTaskPageModel
{
	IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
	bool IsBusy { get; }
}