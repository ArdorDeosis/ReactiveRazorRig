using Microsoft.AspNetCore.Components;

namespace ReactiveRazorRig;

/// <summary>
/// A component that observes an <see cref="IObservable{T}"/> and renders content based on its state.
/// </summary>
/// <typeparam name="T">The type of the observed value.</typeparam>
public sealed class Observe<T> : ComponentBase, IDisposable
{
  private IDisposable? subscription;

  private T value = default!;
  private bool receivedValue;
  private bool completed;
  private Exception? error;

  /// <summary>
  /// The observable source to observe.
  /// </summary>
  [Parameter]
  [EditorRequired]
  public IObservable<T>? Source { get; set; }

  /// <summary>
  /// The initial value to use for rendering before any values are received from the source.
  /// </summary>
  [Parameter]
  public T InitialValue { get; set; } = default!;

  /// <summary>
  /// The content to render.
  /// </summary>
  [Parameter]
  public RenderFragment<T>? ChildContent { get; set; }

  /// <summary>
  /// The content to render if a value has been received from the source or <see cref="InitialValue"/> has been set.
  /// </summary>
  [Parameter]
  public RenderFragment<T>? ValueContent { get; set; }

  /// <summary>
  /// The content to render initially before any values (incl. <see cref="InitialValue"/>) are received.
  /// </summary>
  [Parameter]
  public RenderFragment? InitialContent { get; set; }

  /// <summary>
  /// The content to render when an error occurs.
  /// </summary>
  /// <remarks>The exception emitted by the <see cref="Source"/> can be accessed with <c>@context</c>.</remarks>
  [Parameter]
  public RenderFragment<Exception>? ErrorContent { get; set; }

  /// <summary>
  /// Gets or sets the content to render when the source completes.
  /// </summary>
  [Parameter]
  public RenderFragment? CompletedContent { get; set; }

  /// <inheritdoc />
  public void Dispose() => subscription?.Dispose();

  /// <inheritdoc />
  public override async Task SetParametersAsync(ParameterView parameters)
  {
    if (parameters.TryGetValue<T>(nameof(InitialValue), out var value))
    {
      receivedValue = true;
      this.value = value;
    }

    await base.SetParametersAsync(parameters);
  }

  /// <inheritdoc />
  protected override void OnParametersSet()
  {
    subscription?.Dispose();
    if (Source == null)
      return;
    subscription = Source.Subscribe(HandleNewValue, HandleError, HandleCompleted);
  }

  /// <inheritdoc />
  protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder) => 
    builder.AddContent(0, GetContent());

  /// <summary>
  /// Handles a new value from the observable source.
  /// </summary>
  /// <param name="newValue">The new value to be rendered.</param>
  private void HandleNewValue(T newValue)
  {
    receivedValue = true;
    value = newValue;
    InvokeAsync(StateHasChanged);
  }

  /// <summary>
  /// Handles an error from the observable source.
  /// </summary>
  /// <param name="exception">The exception that occurred.</param>
  private void HandleError(Exception exception)
  {
    if (ErrorContent is null)
      return;
    error = exception;
    InvokeAsync(StateHasChanged);
  }

  /// <summary>
  /// Handles the completion of the observable source.
  /// </summary>
  private void HandleCompleted()
  {
    if (CompletedContent is null)
      return;
    completed = true;
    InvokeAsync(StateHasChanged);
  }

  /// <summary>
  /// Determines the content to be rendered based on the current state.
  /// </summary>
  private RenderFragment? GetContent()
  {
    if (error is not null && ErrorContent is not null) return ErrorContent(error);
    if (completed && CompletedContent is not null) return CompletedContent;
    if (receivedValue) return ValueContent?.Invoke(value) ?? ChildContent?.Invoke(value);
    return InitialContent;
  }
}