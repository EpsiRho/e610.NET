﻿#pragma checksum "C:\Users\jhset\Documents\GitHub\e610.NET\e610.NET\e621.NET\Pages\PoolView.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "FA2FFECBBA8EDC89066F6DB6CD2FE1DFA573DB8352AAAA1A7CAFD55ED0024655"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace e610.NET.Pages
{
    partial class PoolView : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private static class XamlBindingSetters
        {
            public static void Set_Windows_UI_Xaml_Controls_TextBlock_Text(global::Windows.UI.Xaml.Controls.TextBlock obj, global::System.String value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = targetNullValue;
                }
                obj.Text = value ?? global::System.String.Empty;
            }
            public static void Set_Windows_UI_Xaml_Controls_ItemsControl_ItemsSource(global::Windows.UI.Xaml.Controls.ItemsControl obj, global::System.Object value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::System.Object) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::System.Object), targetNullValue);
                }
                obj.ItemsSource = value;
            }
            public static void Set_Windows_UI_Xaml_FrameworkElement_Width(global::Windows.UI.Xaml.FrameworkElement obj, global::System.Double value)
            {
                obj.Width = value;
            }
            public static void Set_Windows_UI_Xaml_FrameworkElement_Height(global::Windows.UI.Xaml.FrameworkElement obj, global::System.Double value)
            {
                obj.Height = value;
            }
        };

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class PoolView_obj16_Bindings :
            global::Windows.UI.Xaml.IDataTemplateExtension,
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            IPoolView_Bindings
        {
            private global::e610.NET.Post dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);
            private bool removedDataContextHandler = false;

            // Fields for each control that has bindings.
            private global::System.WeakReference obj16;
            private global::Microsoft.Toolkit.Uwp.UI.Controls.ImageEx obj17;
            private global::Windows.UI.Xaml.Controls.TextBlock obj18;

            public PoolView_obj16_Bindings()
            {
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 16: // Pages\PoolView.xaml line 51
                        this.obj16 = new global::System.WeakReference((global::Windows.UI.Xaml.Controls.Grid)target);
                        break;
                    case 17: // Pages\PoolView.xaml line 52
                        this.obj17 = (global::Microsoft.Toolkit.Uwp.UI.Controls.ImageEx)target;
                        break;
                    case 18: // Pages\PoolView.xaml line 54
                        this.obj18 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    default:
                        break;
                }
            }

            public void DataContextChangedHandler(global::Windows.UI.Xaml.FrameworkElement sender, global::Windows.UI.Xaml.DataContextChangedEventArgs args)
            {
                 if (this.SetDataRoot(args.NewValue))
                 {
                    this.Update();
                 }
            }

            // IDataTemplateExtension

            public bool ProcessBinding(uint phase)
            {
                throw new global::System.NotImplementedException();
            }

            public int ProcessBindings(global::Windows.UI.Xaml.Controls.ContainerContentChangingEventArgs args)
            {
                int nextPhase = -1;
                ProcessBindings(args.Item, args.ItemIndex, (int)args.Phase, out nextPhase);
                return nextPhase;
            }

            public void ResetTemplate()
            {
                Recycle();
            }

            // IDataTemplateComponent

            public void ProcessBindings(global::System.Object item, int itemIndex, int phase, out int nextPhase)
            {
                nextPhase = -1;
                switch(phase)
                {
                    case 0:
                        nextPhase = 1;
                        this.SetDataRoot(item);
                        if (!removedDataContextHandler)
                        {
                            removedDataContextHandler = true;
                            (this.obj16.Target as global::Windows.UI.Xaml.Controls.Grid).DataContextChanged -= this.DataContextChangedHandler;
                        }
                        this.initialized = true;
                        break;
                    case 1:
                        global::Windows.UI.Xaml.Markup.XamlBindingHelper.ResumeRendering(this.obj18);
                        nextPhase = 2;
                        break;
                    case 2:
                        global::Windows.UI.Xaml.Markup.XamlBindingHelper.ResumeRendering(this.obj17);
                        nextPhase = -1;
                        break;
                }
                this.Update_((global::e610.NET.Post) item, 1 << phase);
            }

            public void Recycle()
            {
                global::Windows.UI.Xaml.Markup.XamlBindingHelper.SuspendRendering(this.obj18);
                global::Windows.UI.Xaml.Markup.XamlBindingHelper.SuspendRendering(this.obj17);
            }

            // IPoolView_Bindings

            public void Initialize()
            {
                if (!this.initialized)
                {
                    this.Update();
                }
            }
            
            public void Update()
            {
                this.Update_(this.dataRoot, NOT_PHASED);
                this.initialized = true;
            }

            public void StopTracking()
            {
            }

            public void DisconnectUnloadedObject(int connectionId)
            {
                throw new global::System.ArgumentException("No unloadable elements to disconnect.");
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                if (newDataRoot != null)
                {
                    this.dataRoot = (global::e610.NET.Post)newDataRoot;
                    return true;
                }
                return false;
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::e610.NET.Post obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0) | (1 << 1))) != 0)
                    {
                        this.Update_file(obj.file, phase);
                    }
                    if ((phase & (NOT_PHASED | (1 << 0) | (1 << 2))) != 0)
                    {
                        this.Update_preview(obj.preview, phase);
                    }
                }
            }
            private void Update_preview(global::e610.NET.Preview obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0) | (1 << 2))) != 0)
                    {
                        this.Update_preview_width(obj.width, phase);
                        this.Update_preview_height(obj.height, phase);
                    }
                }
            }
            private void Update_preview_width(global::System.Int32 obj, int phase)
            {
                if ((phase & ((1 << 2) | NOT_PHASED )) != 0)
                {
                    // Pages\PoolView.xaml line 52
                    XamlBindingSetters.Set_Windows_UI_Xaml_FrameworkElement_Width(this.obj17, obj);
                }
            }
            private void Update_preview_height(global::System.Int32 obj, int phase)
            {
                if ((phase & ((1 << 2) | NOT_PHASED )) != 0)
                {
                    // Pages\PoolView.xaml line 52
                    XamlBindingSetters.Set_Windows_UI_Xaml_FrameworkElement_Height(this.obj17, obj);
                }
            }
            private void Update_file(global::e610.NET.File obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0) | (1 << 1))) != 0)
                    {
                        this.Update_file_ext(obj.ext, phase);
                    }
                }
            }
            private void Update_file_ext(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 1) | NOT_PHASED )) != 0)
                {
                    // Pages\PoolView.xaml line 54
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj18, obj, null);
                }
            }
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class PoolView_obj1_Bindings :
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            IPoolView_Bindings
        {
            private global::e610.NET.Pages.PoolView dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);

            // Fields for each control that has bindings.
            private global::Windows.UI.Xaml.Controls.TextBlock obj10;
            private global::Windows.UI.Xaml.Controls.GridView obj14;

            public PoolView_obj1_Bindings()
            {
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 10: // Pages\PoolView.xaml line 94
                        this.obj10 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    case 14: // Pages\PoolView.xaml line 46
                        this.obj14 = (global::Windows.UI.Xaml.Controls.GridView)target;
                        break;
                    default:
                        break;
                }
            }

            // IPoolView_Bindings

            public void Initialize()
            {
                if (!this.initialized)
                {
                    this.Update();
                }
            }
            
            public void Update()
            {
                this.Update_(this.dataRoot, NOT_PHASED);
                this.initialized = true;
            }

            public void StopTracking()
            {
            }

            public void DisconnectUnloadedObject(int connectionId)
            {
                throw new global::System.ArgumentException("No unloadable elements to disconnect.");
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                if (newDataRoot != null)
                {
                    this.dataRoot = (global::e610.NET.Pages.PoolView)newDataRoot;
                    return true;
                }
                return false;
            }

            public void Loading(global::Windows.UI.Xaml.FrameworkElement src, object data)
            {
                this.Initialize();
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::e610.NET.Pages.PoolView obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_pageCount(obj.pageCount, phase);
                        this.Update_ViewModel(obj.ViewModel, phase);
                    }
                }
            }
            private void Update_pageCount(global::System.Int32 obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // Pages\PoolView.xaml line 94
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj10, obj.ToString(), null);
                }
            }
            private void Update_ViewModel(global::e610.NET.PostsViewModel obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_ViewModel_Posts(obj.Posts, phase);
                    }
                }
            }
            private void Update_ViewModel_Posts(global::System.Collections.ObjectModel.ObservableCollection<global::e610.NET.Post> obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // Pages\PoolView.xaml line 46
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_ItemsControl_ItemsSource(this.obj14, obj, null);
                }
            }
        }
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1: // Pages\PoolView.xaml line 1
                {
                    this.page = (global::Windows.UI.Xaml.Controls.Page)(target);
                }
                break;
            case 2: // Pages\PoolView.xaml line 116
                {
                    this.InfoPopup = (global::Microsoft.UI.Xaml.Controls.InfoBar)(target);
                }
                break;
            case 3: // Pages\PoolView.xaml line 80
                {
                    this.LoadingBar = (global::Microsoft.UI.Xaml.Controls.ProgressBar)(target);
                }
                break;
            case 4: // Pages\PoolView.xaml line 86
                {
                    this.PostCountSlider = (global::Windows.UI.Xaml.Controls.Slider)(target);
                }
                break;
            case 5: // Pages\PoolView.xaml line 99
                {
                    this.DownloadPool = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.DownloadPool).Click += this.DownloadPool_Click;
                }
                break;
            case 6: // Pages\PoolView.xaml line 104
                {
                    this.NormalText = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 7: // Pages\PoolView.xaml line 106
                {
                    this.DownloadProgress = (global::Microsoft.UI.Xaml.Controls.ProgressBar)(target);
                }
                break;
            case 8: // Pages\PoolView.xaml line 109
                {
                    this.FilesText = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 9: // Pages\PoolView.xaml line 91
                {
                    this.BackPage = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.BackPage).Tapped += this.BackPage_Tapped;
                }
                break;
            case 11: // Pages\PoolView.xaml line 95
                {
                    this.ForwardPage = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ForwardPage).Tapped += this.ForwardPage_Tapped;
                }
                break;
            case 12: // Pages\PoolView.xaml line 71
                {
                    this.SearchBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                    ((global::Windows.UI.Xaml.Controls.TextBox)this.SearchBox).KeyDown += this.SearchBox_KeyDown;
                }
                break;
            case 13: // Pages\PoolView.xaml line 74
                {
                    this.SearchButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.SearchButton).Tapped += this.SearchButton_Tapped;
                }
                break;
            case 14: // Pages\PoolView.xaml line 46
                {
                    this.ImageGrid = (global::Windows.UI.Xaml.Controls.GridView)(target);
                    ((global::Windows.UI.Xaml.Controls.GridView)this.ImageGrid).ItemClick += this.ImageGrid_ItemClick;
                }
                break;
            case 19: // Pages\PoolView.xaml line 40
                {
                    this.PoolTitle = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 20: // Pages\PoolView.xaml line 41
                {
                    this.DescToggle = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.DescToggle).Click += this.DescToggle_Click;
                }
                break;
            case 21: // Pages\PoolView.xaml line 42
                {
                    this.PoolDesc = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            switch(connectionId)
            {
            case 1: // Pages\PoolView.xaml line 1
                {                    
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)target;
                    PoolView_obj1_Bindings bindings = new PoolView_obj1_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(this);
                    this.Bindings = bindings;
                    element1.Loading += bindings.Loading;
                }
                break;
            case 16: // Pages\PoolView.xaml line 51
                {                    
                    global::Windows.UI.Xaml.Controls.Grid element16 = (global::Windows.UI.Xaml.Controls.Grid)target;
                    PoolView_obj16_Bindings bindings = new PoolView_obj16_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(element16.DataContext);
                    element16.DataContextChanged += bindings.DataContextChangedHandler;
                    global::Windows.UI.Xaml.DataTemplate.SetExtensionInstance(element16, bindings);
                    global::Windows.UI.Xaml.Markup.XamlBindingHelper.SetDataTemplateComponent(element16, bindings);
                }
                break;
            }
            return returnValue;
        }
    }
}

