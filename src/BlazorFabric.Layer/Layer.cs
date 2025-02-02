﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlazorFabric
{
    // Blazor doesn't have a React equivalent to "createPortal" so to use a Layer, you need to place 
    // a LayerHost manually near the root of the app.  This will allow you to use a CascadeParameter 
    // to send the LayerHost to anywhere in the app and render items to it.

    public class Layer : FabricComponentBase, IDisposable
    {
        //internal LayerBase() { }

        [Inject] private IJSRuntime JSRuntime { get; set; }
        //[Inject] private IComponentContext ComponentContext { get; set; }
        
        [Parameter] public RenderFragment ChildContent { get; set; }

        [CascadingParameter(Name = "HostedContent")] protected LayerHost LayerHost { get; set; }

        private bool addedToHost = false;

        public string id = Guid.NewGuid().ToString();

        protected override Task OnParametersSetAsync()
        {
            if (!addedToHost)
            {
                LayerHost.AddOrUpdateHostedContent(id, ChildContent, Style);
                addedToHost = true;
            }
            return base.OnParametersSetAsync();
        }

        protected override bool ShouldRender()
        {
            if (LayerHost != null) // && ComponentContext.IsConnected)
            {
                LayerHost.AddOrUpdateHostedContent(id, ChildContent, Style);
                addedToHost = true;
            }
            return false;
        }

        //public void Rerender()
        //{
        //    StateHasChanged();
        //}

        protected override void OnAfterRender(bool firstRender)
        {
            //if (layerElement == null)
            base.OnAfterRender(firstRender);
        }

        public void Dispose()
        {
            LayerHost.RemoveHostedContent(this.id);
            addedToHost = false;
        }
    }
}
