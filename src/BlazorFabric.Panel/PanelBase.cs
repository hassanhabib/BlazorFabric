﻿using BlazorFabric.PanelInternal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BlazorFabric
{
    public class PanelBase : FabricComponentBase, IDisposable
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public string CloseButtonAriaLabel { get; set; }

        [Parameter]
        public string CustomWidth { get; set; }

        [Parameter]
        public ElementReference ElementToFocusOnDismiss { get; set; }

        [Parameter]
        public string FirstFocusableSelector { get; set; }

        [Parameter]
        public RenderFragment FooterTemplate { get; set; }

        [Parameter]
        public bool ForceFocusInsideTrap{ get; set; }

        [Parameter]
        public bool HasCloseButton { get; set; } = true;

        [Parameter]
        public string HeaderClassName { get; set; }

        [Parameter]
        public RenderFragment HeaderTemplate { get; set; }

        [Parameter]
        public string HeaderText { get; set; }

        [Parameter]
        public bool IsBlocking { get; set; } = true;

        [Parameter]
        public bool IsFooterAtBottom { get; set; }

        [Parameter]
        public bool IsHiddenOnDismiss { get; set; }

        [Parameter]
        public bool IsLightDismiss { get; set; }

        [Parameter]
        public bool IsOpen { get; set; }

        [Parameter]
        public RenderFragment NavigationTemplate { get; set; }

        [Parameter]
        public RenderFragment NavigationContentTemplate { get; set; }

        [Parameter]
        public EventCallback OnDismiss { get; set; }
   
        [Parameter]
        public EventCallback OnDismissed { get; set; }

        [Parameter]
        public EventCallback OnLightDismissClick { get; set; }

        [Parameter]
        public EventCallback OnOpen { get; set; }

        [Parameter]
        public EventCallback OnOpened { get; set; }

        [Parameter]
        public EventCallback OnOuterClick { get; set; }

        [Parameter]
        public PanelType Type { get; set; } = PanelType.SmallFixedFar;

        protected bool isAnimating = false;
        
        protected EventCallback ThrowawayCallback;

        private PanelVisibilityState previousVisibility = PanelVisibilityState.Closed;
        protected PanelVisibilityState currentVisibility = PanelVisibilityState.Closed;
        protected bool isFooterSticky = false;

        protected Action<MouseEventArgs> onPanelClick;
        private Action dismiss;
        private List<int> _scrollerEventId = new List<int>();
        private int _resizeId = -1;
        private int _mouseDownId = -1;

        private Timer _animationTimer;
        private Action _clearExistingAnimationTimer;
        private Action<PanelVisibilityState> _animateTo;
        private Action _onTransitionComplete;

        protected ElementReference panelElement;
        protected ElementReference scrollableContent;
        private bool _scrollerRegistered;

        ElapsedEventHandler _handler = null;

        public PanelBase()
        {
            _animationTimer = new Timer();

            HeaderTemplate = builder =>
            {
                if (HeaderText != null)
                {
                    builder.OpenElement(0, "div");
                    {
                        builder.AddAttribute(1, "class", "ms-Panel-header");
                        builder.OpenElement(2, "p");
                        {
                            builder.AddAttribute(3, "class", "xlargeFont ms-Panel-headerText");
                            //builder.AddAttribute(4, "id", )
                            builder.AddAttribute(5, "role", "heading");
                            builder.AddAttribute(6, "aria-level", "2");
                            builder.AddContent(7, HeaderText);
                        }
                        builder.CloseElement();
                    }
                    builder.CloseElement();
                }
            };

            onPanelClick = (ev) =>
            {
                this.dismiss();
            };

            dismiss = async () =>
            {
                await OnDismiss.InvokeAsync(null);
                //normally, would check react synth events to see if event was interrupted from the OnDismiss callback before calling the following... 
                // To Do
                this.Close();
            };

            _clearExistingAnimationTimer = () =>
            {
                if (_animationTimer.Enabled)
                {
                    _animationTimer.Stop();
                    _animationTimer.Elapsed -= _handler;
                }
            };

            _animateTo = (animationState) =>
            {
                _animationTimer.Interval = 200;
                _handler = null;
                _handler = (s, e) =>
                {
                    InvokeAsync(() =>
                    {
                        Debug.WriteLine("Inside invokeAsync from animateTo timer elapsed");
                        _animationTimer.Elapsed -= _handler;
                        _animationTimer.Stop();
                        
                        previousVisibility = currentVisibility;
                        currentVisibility = animationState;
                        _onTransitionComplete();
                    });
                };
                _animationTimer.Elapsed += _handler;
                _animationTimer.Start();
            };

            _onTransitionComplete = async () =>
            {
                isAnimating = false;
                await UpdateFooterPositionAsync();

                if (currentVisibility == PanelVisibilityState.Open)
                {
                    await OnOpened.InvokeAsync(null);
                }
                if (currentVisibility == PanelVisibilityState.Closed)
                {
                    await OnDismissed.InvokeAsync(null);
                }
                StateHasChanged();
            };

        }

        [JSInvokable]
        public async Task UpdateFooterPositionAsync()
        {
            Debug.WriteLine("Calling UpdateFooterPositionAsync");
            var clientHeight = await JSRuntime.InvokeAsync<double>("BlazorFabricBaseComponent.getClientHeight", scrollableContent);
            var scrollHeight = await JSRuntime.InvokeAsync<double>("BlazorFabricBaseComponent.getScrollHeight", scrollableContent);

            if (clientHeight < scrollHeight)
                isFooterSticky = true;
            else
                isFooterSticky = false;
        }

        [JSInvokable]
        public async Task DismissOnOuterClick(bool contains)
        {
            if (IsActive())
            {
                Debug.WriteLine("Calling DismissOnOuterClick");
                //var contains = await JSRuntime.InvokeAsync<bool>("BlazorFabricFocusTrapZone.elementContains", panelElement, targetElement);
                if (!contains)
                {
                    await OnOuterClick.InvokeAsync(null);
                    //need to prevent default for bubbling maybe.  Test with lightdismiss ...
                }
                else
                {
                    dismiss();
                }
            }
        }

        public void Open()
        {
            //ignore these calls if we have isOpen set... isOpen need to be nullable in this case... 
            // To Do

        }

        public void Close()
        {
            //ignore these calls if we have isOpen set... isOpen need to be nullable in this case... 
            // To Do
        }

        protected string GetTypeCss()
        {
            switch (Type)
            {
                case PanelType.SmallFixedNear:
                    return " ms-Panel--smLeft";
                case PanelType.SmallFixedFar:
                    return " ms-Panel--sm";
                case PanelType.SmallFluid:
                    return " ms-Panel--smFluid";
                case PanelType.Medium:
                    return " ms-Panel--md";
                case PanelType.Large:
                    return " ms-Panel--lg";
                case PanelType.LargeFixed:
                    return " ms-Panel--fixed";
                case PanelType.ExtraLarge:
                    return " ms-Panel--xl";
                case PanelType.Custom:
                    return " ms-Panel--custom";
                case PanelType.CustomNear:
                    return " ms-Panel--customLeft";
                default:
                    return "";
            }
        }

        protected string GetMainAnimation()
        {
            bool isOnRightSide = true;
            switch (Type)
            { 
                // this changes in RTL env, To Do
                case PanelType.SmallFixedNear:
                case PanelType.CustomNear:
                    isOnRightSide = false;
                    break;
            }
            if (IsOpen && isAnimating && !isOnRightSide)
                return " slideRightIn40";
            if (IsOpen && isAnimating && isOnRightSide)
                return " slideLeftIn40";
            if (!IsOpen && isAnimating && !isOnRightSide)
                return " slideLeftOut40";
            if (!IsOpen && isAnimating && isOnRightSide)
                return " slideRightOut40";
            return "";
        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            Debug.WriteLine("OnAfterRenderAsync");

            if (firstRender)
            {
                
                
                if (ShouldListenForOuterClick())
                {
                    _mouseDownId = await JSRuntime.InvokeAsync<int>("BlazorFabricPanel.registerMouseDownHandler", panelElement, DotNetObjectReference.Create(this));
                }

                if (IsOpen)
                {
                    currentVisibility = PanelVisibilityState.AnimatingOpen;
                }

            }

            if (ShouldListenForOuterClick() && _mouseDownId == -1)
            {
                _mouseDownId = await JSRuntime.InvokeAsync<int>("BlazorFabricPanel.registerMouseDownHandler", panelElement, DotNetObjectReference.Create(this));
            }
            else if (!ShouldListenForOuterClick() && _mouseDownId != -1)
            {
                await JSRuntime.InvokeVoidAsync("BlazorFabricPanel.unregisterHandler", _mouseDownId);
            }

            if (IsOpen && _resizeId == -1)
            {
                _resizeId = await JSRuntime.InvokeAsync<int>("BlazorFabricPanel.registerSizeHandler", DotNetObjectReference.Create(this));
                //listen for lightdismiss
            }
            else if (!IsOpen && _resizeId != -1)
            {
                await JSRuntime.InvokeVoidAsync("BlazorFabricPanel.unregisterHandler", _resizeId);
            }

            if (IsOpen && !_scrollerRegistered)
            {
                _scrollerRegistered = true;
                Debug.WriteLine("Registering scrollableContent");
                _scrollerEventId = await JSRuntime.InvokeAsync<List<int>>("BlazorFabricPanel.makeElementScrollAllower", scrollableContent);
            }

            if (!IsOpen && _scrollerRegistered)
            {
                _scrollerRegistered = false;
                //var tempArray = _scrollerEventId.ToArray();
                //_scrollerEventId.Clear();
                foreach (var id in _scrollerEventId)
                {
                    await JSRuntime.InvokeVoidAsync("BlazorFabricPanel.unregisterHandler", id);
                }
                _scrollerEventId.Clear();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        protected override async Task OnParametersSetAsync()
        {
            Debug.WriteLine("OnParametersSetAsync");
            previousVisibility = currentVisibility;
            if (IsOpen && (currentVisibility == PanelVisibilityState.Closed || currentVisibility == PanelVisibilityState.AnimatingClosed))
            {
                currentVisibility = PanelVisibilityState.AnimatingOpen;
            }
            if (!IsOpen && (currentVisibility == PanelVisibilityState.Open || currentVisibility == PanelVisibilityState.AnimatingOpen))
            {
                currentVisibility = PanelVisibilityState.AnimatingClosed;
            }


            if (currentVisibility != previousVisibility)
            {
                _clearExistingAnimationTimer();
                if (currentVisibility == PanelVisibilityState.AnimatingOpen)
                {
                    isAnimating = true;
                    _animateTo(PanelVisibilityState.Open);
                }
                else if (currentVisibility == PanelVisibilityState.AnimatingClosed)
                {
                    Debug.WriteLine("setting isAnimating to true");
                    isAnimating = true;
                    Debug.WriteLine("invoking _animateTo(closed)");
                    _animateTo(PanelVisibilityState.Closed);
                }
            }

            

            await base.OnParametersSetAsync();
        }

        private bool IsActive()
        {
            return currentVisibility == PanelVisibilityState.Open || currentVisibility == PanelVisibilityState.AnimatingOpen;
        }

        private bool ShouldListenForOuterClick()
        {
            return IsBlocking && IsOpen;
        }

        public async void Dispose()
        {
            if (_scrollerEventId != null)
            {
                foreach (var id in _scrollerEventId)
                {
                    Debug.WriteLine("Removing scrollerEvent");
                    await JSRuntime.InvokeVoidAsync("BlazorFabricPanel.unregisterHandler", id);
                }
                _scrollerEventId.Clear();
            }

            if (_resizeId != -1)
            {
                Debug.WriteLine("Removing resizeEvent");
                await JSRuntime.InvokeVoidAsync("BlazorFabricPanel.unregisterHandler", _resizeId);
            }
            if (_mouseDownId != -1)
            {
                Debug.WriteLine("Removing mouseDownEvent");
                await JSRuntime.InvokeVoidAsync("BlazorFabricPanel.unregisterHandler", _mouseDownId);
            }
        }
    }
}
