using Microsoft.AspNetCore.Components;

namespace ReactiveRazorRig;

public static class Extensions
{
  /// <summary>
  /// Creates a <see cref="RenderFragment"/> that renders values from the specified observable source.
  /// </summary>
  /// <typeparam name="T">The type of the values provided by the observable source.</typeparam>
  /// <param name="observable">The observable source that provides the values.</param>
  /// <param name="initialValue">
  /// The initial value to be rendered before any values are received from the observable source.
  /// </param>
  /// <returns>A <see cref="RenderFragment"/> that renders the values from the observable source.</returns>
  public static RenderFragment Observe<T>(this IObservable<T>? observable, object? initialValue = null) where T : notnull =>
    builder =>
    {
      builder.OpenComponent<BindObservable<T>>(0);
      builder.AddComponentParameter(1, nameof(BindObservable<T>.Source), observable);
      builder.AddComponentParameter(2, nameof(BindObservable<T>.Value), initialValue);
      builder.CloseComponent();
    };
}