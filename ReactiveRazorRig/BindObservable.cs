using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ReactiveRazorRig;

/// <summary>
/// A component that subscribes to an <see cref="IObservable{T}"/> and renders its value.
/// </summary>
/// <typeparam name="T">The type of the value provided by the observable source.</typeparam>
/// <remarks>Intended to be used through the extension method <see cref="Extensions.Observe{T}"/>.</remarks>
internal sealed class BindObservable<T> : ComponentBase, IDisposable
{
  private IDisposable? subscription;

  /// <summary>
  /// The observable source that provides values to be rendered.
  /// </summary>
  [Parameter]
  public IObservable<T>? Source { get; set; }

  /// <summary>
  /// The currently rendered value. Can be used to set an initial value to be rendered before <see cref="Source"/>
  /// emitted any values.
  /// </summary>
  [Parameter]
  public object? Value { get; set; }

  /// <inheritdoc />
  public void Dispose() => subscription?.Dispose();

  /// <inheritdoc />
  protected override void OnParametersSet()
  {
    subscription?.Dispose();
    subscription = Source?.Subscribe(HandleValueChange);
  }

  /// <inheritdoc />
  protected override void BuildRenderTree(RenderTreeBuilder builder) => 
    builder.AddContent(0, Value);

  /// <summary>
  /// Handles a new value from the observable source.
  /// </summary>
  /// <param name="value">The new value to be rendered.</param>
  private void HandleValueChange(T value)
  {
    if (Value is T typedValue && EqualityComparer<T>.Default.Equals(typedValue, value))
      return;
    Value = value;
    InvokeAsync(StateHasChanged);
  }
}