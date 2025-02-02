﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BlazorFabric
{
    public class TooltipHostBase : FabricComponentBase, IDisposable
    {
        private static TooltipHostBase CurrentVisibleTooltip { get; set; }

        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public double CloseDelay { get; set; } = double.NaN;
        [Parameter] public TooltipDelay Delay { get; set; } = TooltipDelay.Medium;
        [Parameter] public DirectionalHint DirectionalHint { get; set; }
        //[Parameter] public FabricComponentBase FabricComponentTarget { get; set; }
        [Parameter] public string HostClassName { get; set; }
        [Parameter] public EventCallback<bool> OnTooltipToggle { get; set; }
        [Parameter] public TooltipOverflowMode OverflowMode { get; set; } = TooltipOverflowMode.None;  
        [Parameter] public FabricComponentBase Parent { get; set; }
        [Parameter] public bool SetAriaDescribedBy { get; set; }
        [Parameter] public RenderFragment TooltipContent { get; set; }

        protected FabricComponentBase TargetElement;
        protected bool ShowTooltip;

        protected bool IsTooltipVisible = false;
        protected bool IsAriaPlaceholderRendered = false;

        private Timer _openTimer;
        private Timer _dismissTimer;

        public TooltipHostBase()
        {
            _openTimer = new Timer();
            _openTimer.Elapsed += _openTimer_Elapsed;
            _dismissTimer = new Timer();
            _dismissTimer.Elapsed += _dismissTimer_Elapsed;
        }

        private void _openTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _openTimer.Stop();
            ToggleTooltip(true);
        }

        private void _dismissTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _dismissTimer.Stop();
            ToggleTooltip(false);
        }

        public void Show()
        {
            ToggleTooltip(true);
        }

        public void Dismiss()
        {
            ToggleTooltip(false);
        }

        protected override Task OnParametersSetAsync()
        {
            DetermineTargetElement();
            return base.OnParametersSetAsync();
        }

        protected Task OnTooltipMouseEnter(EventArgs args)
        {
            Debug.WriteLine("OnMouseEnter");
            if (TooltipHostBase.CurrentVisibleTooltip != null && TooltipHostBase.CurrentVisibleTooltip != this)
                TooltipHostBase.CurrentVisibleTooltip.Dismiss();

            TooltipHostBase.CurrentVisibleTooltip = this;

            if (OverflowMode != TooltipOverflowMode.None)
            {
                DetermineTargetElement();
                // test for overflow... return and do nothing if there is none
                // for now, let's not show the tooltip until the detection for overflow works
                return Task.CompletedTask;
            }

            // do another test to see if tooltip target is inside a portal relative to the tooltiphost.  Probably won't deal with this for a while.
            // return and do nothing if so...

            _dismissTimer.Stop();
            _openTimer.Stop();

            if (Delay != TooltipDelay.Zero)
            {
                IsAriaPlaceholderRendered = true;
                var delayTime = GetDelayTime(Delay);
                _openTimer.Interval = delayTime;
                _openTimer.Start();
            }
            else
            {
                ToggleTooltip(true);
            }
            
            return Task.CompletedTask;
        }

        protected Task OnTooltipMouseLeave(EventArgs args)
        {
            Debug.WriteLine("OnMouseLeave");
            _dismissTimer.Stop();
            _openTimer.Stop();

            if (!double.IsNaN(CloseDelay))
            {
                _dismissTimer.Interval = CloseDelay;
                _dismissTimer.Start();
            }
            else
            {
                ToggleTooltip(false);
            }

            if (TooltipHostBase.CurrentVisibleTooltip == this)
            {
                TooltipHostBase.CurrentVisibleTooltip = null;
            }

            return Task.CompletedTask;
        }

        protected void HideTooltip()
        {
            ToggleTooltip(false);
        }

        protected Task OnTooltipKeyDown(KeyboardEventArgs args)
        {
            if (args.Code == "Esc")
                HideTooltip();
            return Task.CompletedTask;
        }

        private void ToggleTooltip(bool isOpen)
        {
            Debug.WriteLine($"Toggling tooltip: {isOpen}");
            InvokeAsync(() =>
            {
                IsTooltipVisible = isOpen;
                IsAriaPlaceholderRendered = false;
                StateHasChanged();
                Debug.WriteLine($"Toggling statehaschanged");

                OnTooltipToggle.InvokeAsync(isOpen);
            });
        }

        private void DetermineTargetElement()
        {
            if (OverflowMode == TooltipOverflowMode.Parent && Parent != null)
                TargetElement = Parent;
            else
                TargetElement = this;
        }

        private double GetDelayTime(TooltipDelay delay)
        {
            switch (delay)
            {
                case TooltipDelay.Medium:
                    return 300;
                case TooltipDelay.Long:
                    return 500;
                default:
                    return 0;
            }
        }

        public void Dispose()
        {
            if (TooltipHostBase.CurrentVisibleTooltip == this)
                TooltipHostBase.CurrentVisibleTooltip = null;
        }
    }
}
