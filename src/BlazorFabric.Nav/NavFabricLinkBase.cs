﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlazorFabric
{
    public class NavFabricLinkBase : FabricComponentBase
    { 
        [Inject] protected NavigationManager NavigationManager { get; set; }
        
        [Parameter] public RenderFragment ChildContent { get; set; }  //LINKS

        [Parameter] public string AriaLabel { get; set; }
        [Parameter] public bool Disabled { get; set; }
        [Parameter] public bool ForceAnchor { get; set; }
        [Parameter] public string Icon { get; set; }
        [Parameter] public bool IsButton { get; set; }
        [Parameter] public string Name { get; set; }
        [Parameter] public string Target { get; set; }  //link <a> target
        [Parameter] public string Title { get; set; } //tooltip and ARIA
        [Parameter] public string Id { get; set; }
        [Parameter] public string Url { get; set; }

        [Parameter] public int NestedDepth { get; set; }

        [Parameter] public EventCallback<NavFabricLinkBase> OnClick { get; set; }

        [CascadingParameter(Name="ClearSelectionAction")] Action ClearSelectionAction { get; set; }

        protected bool isExpanded { get; set; }


        protected bool isSelected { get; set; }
        protected string depthClass = "";
       
       
        protected override Task OnInitializedAsync()
        {
            System.Diagnostics.Debug.WriteLine("Initializing NavFabricLinkBase");
            ProcessUri(NavigationManager.Uri);
            NavigationManager.LocationChanged += UriHelper_OnLocationChanged;
            return base.OnInitializedAsync();
        }
                

        private void UriHelper_OnLocationChanged(object sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        {
            ProcessUri(e.Location);
        }

        private void ProcessUri(string uri)
        {
            if (uri.EndsWith(this.Id) && !isSelected)
            {
                isSelected = true;
                StateHasChanged();
            }
            else if (!uri.EndsWith(this.Id) && isSelected)
            {
                isSelected = false;
                StateHasChanged();
            }
        }

        protected override Task OnParametersSetAsync()
        {
            switch (this.NestedDepth)
            {
                case 0:
                    depthClass = "";
                    break;
                case 1:
                    depthClass = "depth-one";
                    break;
                case 2:
                    depthClass = "depth-two";
                    break;
                case 3:
                    depthClass = "depth-three";
                    break;
                case 4:
                    depthClass = "depth-four";
                    break;
                case 5:
                    depthClass = "depth-five";
                    break;
                case 6:
                    depthClass = "depth-six";
                    break;
            }

           

            return base.OnParametersSetAsync();
        }

        protected Task ExpandHandler(MouseEventArgs args)
        {
            this.isExpanded = !this.isExpanded;
            return Task.CompletedTask;
        }

        protected async Task ClickHandler(MouseEventArgs args)
        {          
            await OnClick.InvokeAsync(this);
        }
    }
}
