# Blazor Render Loop Detection

A lightweight Blazor Server (SSR) solution that helps detect excessive re-renders (often referred to as *render loops*). Render loops can crash your app by exhausting server resources, especially in Blazor Server, where every connected client shares server memory.

## Overview

This repository provides a **base component** that:

1. Tracks how many renders occur per second (`renderCount`).
2. Raises an alert (via `Debugger.Break()` in debug mode) if the number of renders exceeds a configurable threshold (`renderThresholdPerSecond`).
3. Resets the counter every second, preventing false positives over longer periods.

> **Warning**  
> If you encounter a break in your debugger due to a detected render loop, **check your call stack** to see where `StateHasChanged()` (or other re-render triggers) might be causing repeated rendering. Infinite render loops can cause production crashes or lead to unresponsive applications if left unresolved.

## Why You Need This

- **Early Detection**: Quickly discover problematic code that causes runaway re-renders, often due to multiple or circular `StateHasChanged()` calls.  
- **Production Stability**: Helps prevent your server from running out of memory, an especially critical concern in Blazor Server apps where one user can affect all connected users.  
- **Diagnostics**: Provides hooks to log warnings or errors in telemetry (e.g., Application Insights, log files) when an issue is detected in production.

## How It Works

1. **Render Count**  
   Each time the component finishes `OnAfterRender`, it increments a `renderCount`.  

2. **Threshold Check**  
   If the count exceeds `renderThresholdPerSecond` within a one-second window, the method `OnRenderLoopDetected()` is called.  

3. **Debugger Break / Logging**  
   - In Debug mode (`IsDebugMode()`), the code calls `Debugger.Break()`, pausing execution so you can inspect the call stack.  
   - In Production mode, you could log the event and possibly send an alert to your monitoring system.

4. **Automatic Reset**  
   A `System.Timers.Timer` resets the `renderCount` once per second to prevent false positives that might arise over longer durations.

## Usage
Take inspiration on the component in `Layout/ComponentBase`

## Highlighted code

Core script that simply detects render loops:
```
@namespace Blazor_RenderLoopDetection.Components.Layout
@using System.Timers
@using System.Diagnostics

@code {
    private int renderCount = 0;
    private int renderThresholdPerSecond = 100;

    private Timer _resetRenderCountTimer = new Timer();
    private bool IsRenderThresholdReached() => renderCount > renderThresholdPerSecond;
    private bool IsDebugMode() => true;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        InitializeResetRenderCounterTimer();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        renderCount++;

        if (IsRenderThresholdReached())
            OnRenderLoopDetected();
    }

    private void OnRenderLoopDetected()
    {
        Console.WriteLine("Render loop detected!");

        if (IsDebugMode())
            Debugger.Break(); // Development: Render loop detected!
        else
            LogRenderLoop();
    }

    private void LogRenderLoop()
    {
        // Log the render loop somewhere that it can be monitored
    }

    public void InitializeResetRenderCounterTimer()
    {
        _resetRenderCountTimer.Elapsed += OnResetRenderTimerTimed;
        _resetRenderCountTimer.Interval = 1000;
        _resetRenderCountTimer.Enabled = true;
    }

    public void OnResetRenderTimerTimed(object source, ElapsedEventArgs e)
        => ResetRenderCount();

    private void ResetRenderCount()
        => renderCount = 0;
}
```
