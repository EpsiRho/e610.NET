﻿#pragma checksum "C:\Users\jhset\source\repos\e621.NET\e621.NET\SinglePostView.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "11EC54FE6C4A4DE2B4169B442DCCAC857D4D9C799A0EA92C3E2BA17D7267899E"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace e621.NET
{
    partial class SinglePostView : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private static class XamlBindingSetters
        {
            public static void Set_Microsoft_UI_Xaml_Controls_TeachingTip_Target(global::Microsoft.UI.Xaml.Controls.TeachingTip obj, global::Windows.UI.Xaml.FrameworkElement value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::Windows.UI.Xaml.FrameworkElement) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::Windows.UI.Xaml.FrameworkElement), targetNullValue);
                }
                obj.Target = value;
            }
            public static void Set_Windows_UI_Xaml_Controls_ItemsControl_ItemsSource(global::Windows.UI.Xaml.Controls.ItemsControl obj, global::System.Object value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::System.Object) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::System.Object), targetNullValue);
                }
                obj.ItemsSource = value;
            }
            public static void Set_Windows_UI_Xaml_Controls_Image_Source(global::Windows.UI.Xaml.Controls.Image obj, global::Windows.UI.Xaml.Media.ImageSource value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::Windows.UI.Xaml.Media.ImageSource) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::Windows.UI.Xaml.Media.ImageSource), targetNullValue);
                }
                obj.Source = value;
            }
            public static void Set_Windows_UI_Xaml_Controls_MediaPlayerElement_PosterSource(global::Windows.UI.Xaml.Controls.MediaPlayerElement obj, global::Windows.UI.Xaml.Media.ImageSource value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::Windows.UI.Xaml.Media.ImageSource) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::Windows.UI.Xaml.Media.ImageSource), targetNullValue);
                }
                obj.PosterSource = value;
            }
            public static void Set_Windows_UI_Xaml_Controls_TreeView_ItemsSource(global::Windows.UI.Xaml.Controls.TreeView obj, global::System.Object value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::System.Object) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::System.Object), targetNullValue);
                }
                obj.ItemsSource = value;
            }
            public static void Set_Windows_UI_Xaml_Controls_TextBlock_Text(global::Windows.UI.Xaml.Controls.TextBlock obj, global::System.String value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = targetNullValue;
                }
                obj.Text = value ?? global::System.String.Empty;
            }
            public static void Set_Windows_UI_Xaml_Controls_TreeViewItem_ItemsSource(global::Windows.UI.Xaml.Controls.TreeViewItem obj, global::System.Object value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::System.Object) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::System.Object), targetNullValue);
                }
                obj.ItemsSource = value;
            }
            public static void Set_Windows_UI_Xaml_Controls_ContentControl_Content(global::Windows.UI.Xaml.Controls.ContentControl obj, global::System.Object value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::System.Object) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::System.Object), targetNullValue);
                }
                obj.Content = value;
            }
        };

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class SinglePostView_obj5_Bindings :
            global::Windows.UI.Xaml.IDataTemplateExtension,
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IXamlBindScopeDiagnostics,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            ISinglePostView_Bindings
        {
            private global::e621.NET.Pool dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);
            private bool removedDataContextHandler = false;

            // Fields for each control that has bindings.
            private global::System.WeakReference obj5;

            // Static fields for each binding's enabled/disabled state
            private static bool isobj5TextDisabled = false;

            public SinglePostView_obj5_Bindings()
            {
            }

            public void Disable(int lineNumber, int columnNumber)
            {
                if (lineNumber == 102 && columnNumber == 36)
                {
                    isobj5TextDisabled = true;
                }
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 5: // SinglePostView.xaml line 102
                        this.obj5 = new global::System.WeakReference((global::Windows.UI.Xaml.Controls.TextBlock)target);
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
                        nextPhase = -1;
                        this.SetDataRoot(item);
                        if (!removedDataContextHandler)
                        {
                            removedDataContextHandler = true;
                            (this.obj5.Target as global::Windows.UI.Xaml.Controls.TextBlock).DataContextChanged -= this.DataContextChangedHandler;
                        }
                        this.initialized = true;
                        break;
                }
                this.Update_((global::e621.NET.Pool) item, 1 << phase);
            }

            public void Recycle()
            {
            }

            // ISinglePostView_Bindings

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
                    this.dataRoot = (global::e621.NET.Pool)newDataRoot;
                    return true;
                }
                return false;
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::e621.NET.Pool obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_name(obj.name, phase);
                    }
                }
            }
            private void Update_name(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // SinglePostView.xaml line 102
                    if (!isobj5TextDisabled)
                    {
                        if ((this.obj5.Target as global::Windows.UI.Xaml.Controls.TextBlock) != null)
                        {
                            XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text((this.obj5.Target as global::Windows.UI.Xaml.Controls.TextBlock), obj, null);
                        }
                    }
                }
            }
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class SinglePostView_obj17_Bindings :
            global::Windows.UI.Xaml.IDataTemplateExtension,
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IXamlBindScopeDiagnostics,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            ISinglePostView_Bindings
        {
            private global::e621.NET.TreeItem dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);
            private bool removedDataContextHandler = false;

            // Fields for each control that has bindings.
            private global::System.WeakReference obj17;

            // Static fields for each binding's enabled/disabled state
            private static bool isobj17ItemsSourceDisabled = false;
            private static bool isobj17ContentDisabled = false;

            public SinglePostView_obj17_Bindings()
            {
            }

            public void Disable(int lineNumber, int columnNumber)
            {
                if (lineNumber == 61 && columnNumber == 48)
                {
                    isobj17ItemsSourceDisabled = true;
                }
                else if (lineNumber == 62 && columnNumber == 40)
                {
                    isobj17ContentDisabled = true;
                }
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 17: // SinglePostView.xaml line 61
                        this.obj17 = new global::System.WeakReference((global::Windows.UI.Xaml.Controls.TreeViewItem)target);
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
                        nextPhase = -1;
                        this.SetDataRoot(item);
                        if (!removedDataContextHandler)
                        {
                            removedDataContextHandler = true;
                            (this.obj17.Target as global::Windows.UI.Xaml.Controls.TreeViewItem).DataContextChanged -= this.DataContextChangedHandler;
                        }
                        this.initialized = true;
                        break;
                }
                this.Update_((global::e621.NET.TreeItem) item, 1 << phase);
            }

            public void Recycle()
            {
            }

            // ISinglePostView_Bindings

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
                    this.dataRoot = (global::e621.NET.TreeItem)newDataRoot;
                    return true;
                }
                return false;
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::e621.NET.TreeItem obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_Children(obj.Children, phase);
                        this.Update_Name(obj.Name, phase);
                    }
                }
            }
            private void Update_Children(global::System.Collections.ObjectModel.ObservableCollection<global::e621.NET.TreeItem> obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // SinglePostView.xaml line 61
                    if (!isobj17ItemsSourceDisabled)
                    {
                        if ((this.obj17.Target as global::Windows.UI.Xaml.Controls.TreeViewItem) != null)
                        {
                            XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TreeViewItem_ItemsSource((this.obj17.Target as global::Windows.UI.Xaml.Controls.TreeViewItem), obj, null);
                        }
                    }
                }
            }
            private void Update_Name(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // SinglePostView.xaml line 61
                    if (!isobj17ContentDisabled)
                    {
                        if ((this.obj17.Target as global::Windows.UI.Xaml.Controls.TreeViewItem) != null)
                        {
                            XamlBindingSetters.Set_Windows_UI_Xaml_Controls_ContentControl_Content((this.obj17.Target as global::Windows.UI.Xaml.Controls.TreeViewItem), obj, null);
                        }
                    }
                }
            }
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class SinglePostView_obj1_Bindings :
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IXamlBindScopeDiagnostics,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            ISinglePostView_Bindings
        {
            private global::e621.NET.SinglePostView dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);

            // Fields for each control that has bindings.
            private global::Microsoft.UI.Xaml.Controls.TeachingTip obj2;
            private global::Windows.UI.Xaml.Controls.ListView obj3;
            private global::Windows.UI.Xaml.Controls.Image obj6;
            private global::Windows.UI.Xaml.Controls.MediaPlayerElement obj7;
            private global::Windows.UI.Xaml.Controls.TreeView obj15;

            // Static fields for each binding's enabled/disabled state
            private static bool isobj2TargetDisabled = false;
            private static bool isobj3ItemsSourceDisabled = false;
            private static bool isobj6SourceDisabled = false;
            private static bool isobj7PosterSourceDisabled = false;
            private static bool isobj15ItemsSourceDisabled = false;

            public SinglePostView_obj1_Bindings()
            {
            }

            public void Disable(int lineNumber, int columnNumber)
            {
                if (lineNumber == 97 && columnNumber == 30)
                {
                    isobj2TargetDisabled = true;
                }
                else if (lineNumber == 99 && columnNumber == 23)
                {
                    isobj3ItemsSourceDisabled = true;
                }
                else if (lineNumber == 69 && columnNumber == 40)
                {
                    isobj6SourceDisabled = true;
                }
                else if (lineNumber == 71 && columnNumber == 55)
                {
                    isobj7PosterSourceDisabled = true;
                }
                else if (lineNumber == 58 && columnNumber == 32)
                {
                    isobj15ItemsSourceDisabled = true;
                }
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 2: // SinglePostView.xaml line 95
                        this.obj2 = (global::Microsoft.UI.Xaml.Controls.TeachingTip)target;
                        break;
                    case 3: // SinglePostView.xaml line 99
                        this.obj3 = (global::Windows.UI.Xaml.Controls.ListView)target;
                        break;
                    case 6: // SinglePostView.xaml line 69
                        this.obj6 = (global::Windows.UI.Xaml.Controls.Image)target;
                        break;
                    case 7: // SinglePostView.xaml line 70
                        this.obj7 = (global::Windows.UI.Xaml.Controls.MediaPlayerElement)target;
                        break;
                    case 15: // SinglePostView.xaml line 57
                        this.obj15 = (global::Windows.UI.Xaml.Controls.TreeView)target;
                        break;
                    default:
                        break;
                }
            }

            // IDataTemplateComponent

            public void ProcessBindings(global::System.Object item, int itemIndex, int phase, out int nextPhase)
            {
                nextPhase = -1;
            }

            public void Recycle()
            {
                return;
            }

            // ISinglePostView_Bindings

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
                    this.dataRoot = (global::e621.NET.SinglePostView)newDataRoot;
                    return true;
                }
                return false;
            }

            public void Loading(global::Windows.UI.Xaml.FrameworkElement src, object data)
            {
                this.Initialize();
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::e621.NET.SinglePostView obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_PoolBar(obj.PoolBar, phase);
                        this.Update_ConnectedPools(obj.ConnectedPools, phase);
                        this.Update_singlePost(obj.singlePost, phase);
                        this.Update_DataSource(obj.DataSource, phase);
                    }
                }
            }
            private void Update_PoolBar(global::Windows.UI.Xaml.Controls.Button obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // SinglePostView.xaml line 95
                    if (!isobj2TargetDisabled)
                    {
                        XamlBindingSetters.Set_Microsoft_UI_Xaml_Controls_TeachingTip_Target(this.obj2, obj, null);
                    }
                }
            }
            private void Update_ConnectedPools(global::System.Collections.ObjectModel.ObservableCollection<global::e621.NET.Pool> obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // SinglePostView.xaml line 99
                    if (!isobj3ItemsSourceDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_ItemsControl_ItemsSource(this.obj3, obj, null);
                    }
                }
            }
            private void Update_singlePost(global::e621.NET.Post obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_singlePost_file(obj.file, phase);
                        this.Update_singlePost_preview(obj.preview, phase);
                    }
                }
            }
            private void Update_singlePost_file(global::e621.NET.File obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_singlePost_file_url(obj.url, phase);
                    }
                }
            }
            private void Update_singlePost_file_url(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // SinglePostView.xaml line 69
                    if (!isobj6SourceDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_Image_Source(this.obj6, (global::Windows.UI.Xaml.Media.ImageSource) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::Windows.UI.Xaml.Media.ImageSource), obj), null);
                    }
                }
            }
            private void Update_singlePost_preview(global::e621.NET.Preview obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_singlePost_preview_url(obj.url, phase);
                    }
                }
            }
            private void Update_singlePost_preview_url(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // SinglePostView.xaml line 70
                    if (!isobj7PosterSourceDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_MediaPlayerElement_PosterSource(this.obj7, (global::Windows.UI.Xaml.Media.ImageSource) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::Windows.UI.Xaml.Media.ImageSource), obj), null);
                    }
                }
            }
            private void Update_DataSource(global::System.Collections.ObjectModel.ObservableCollection<global::e621.NET.TreeItem> obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // SinglePostView.xaml line 57
                    if (!isobj15ItemsSourceDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TreeView_ItemsSource(this.obj15, obj, null);
                    }
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
            case 2: // SinglePostView.xaml line 95
                {
                    this.PoolsPopout = (global::Microsoft.UI.Xaml.Controls.TeachingTip)(target);
                }
                break;
            case 6: // SinglePostView.xaml line 69
                {
                    this.bigpicture = (global::Windows.UI.Xaml.Controls.Image)(target);
                    ((global::Windows.UI.Xaml.Controls.Image)this.bigpicture).ImageOpened += this.bigpicture_ImageOpened;
                }
                break;
            case 7: // SinglePostView.xaml line 70
                {
                    this.bigvideo = (global::Windows.UI.Xaml.Controls.MediaPlayerElement)(target);
                }
                break;
            case 8: // SinglePostView.xaml line 75
                {
                    this.VoteUpButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.VoteUpButton).Tapped += this.VoteUpButton_Tapped;
                }
                break;
            case 9: // SinglePostView.xaml line 81
                {
                    this.VoteDownButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.VoteDownButton).Tapped += this.VoteDownButton_Tapped;
                }
                break;
            case 10: // SinglePostView.xaml line 87
                {
                    this.FavoiteButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.FavoiteButton).Tapped += this.FavoiteButton_Tapped;
                }
                break;
            case 11: // SinglePostView.xaml line 84
                {
                    this.VoteDownCount = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 12: // SinglePostView.xaml line 78
                {
                    this.VoteUpCount = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 13: // SinglePostView.xaml line 42
                {
                    this.PoolBar = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.PoolBar).Tapped += this.PoolBar_Tapped;
                }
                break;
            case 14: // SinglePostView.xaml line 53
                {
                    this.LoadingBar = (global::Windows.UI.Xaml.Controls.ProgressBar)(target);
                }
                break;
            case 15: // SinglePostView.xaml line 57
                {
                    this.TagsView = (global::Windows.UI.Xaml.Controls.TreeView)(target);
                    ((global::Windows.UI.Xaml.Controls.TreeView)this.TagsView).ItemInvoked += this.TagsView_ItemInvoked;
                }
                break;
            case 17: // SinglePostView.xaml line 61
                {
                    global::Windows.UI.Xaml.Controls.TreeViewItem element17 = (global::Windows.UI.Xaml.Controls.TreeViewItem)(target);
                    ((global::Windows.UI.Xaml.Controls.TreeViewItem)element17).RightTapped += this.TagsView_RightTapped;
                }
                break;
            case 18: // SinglePostView.xaml line 46
                {
                    this.SearchBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                    ((global::Windows.UI.Xaml.Controls.TextBox)this.SearchBox).KeyDown += this.SearchBox_KeyDown;
                }
                break;
            case 19: // SinglePostView.xaml line 49
                {
                    this.SearchButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.SearchButton).Tapped += this.SearchButton_Tapped;
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
            case 1: // SinglePostView.xaml line 1
                {                    
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)target;
                    SinglePostView_obj1_Bindings bindings = new SinglePostView_obj1_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(this);
                    this.Bindings = bindings;
                    element1.Loading += bindings.Loading;
                    global::Windows.UI.Xaml.Markup.XamlBindingHelper.SetDataTemplateComponent(element1, bindings);
                }
                break;
            case 5: // SinglePostView.xaml line 102
                {                    
                    global::Windows.UI.Xaml.Controls.TextBlock element5 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                    SinglePostView_obj5_Bindings bindings = new SinglePostView_obj5_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(element5.DataContext);
                    element5.DataContextChanged += bindings.DataContextChangedHandler;
                    global::Windows.UI.Xaml.DataTemplate.SetExtensionInstance(element5, bindings);
                    global::Windows.UI.Xaml.Markup.XamlBindingHelper.SetDataTemplateComponent(element5, bindings);
                }
                break;
            case 17: // SinglePostView.xaml line 61
                {                    
                    global::Windows.UI.Xaml.Controls.TreeViewItem element17 = (global::Windows.UI.Xaml.Controls.TreeViewItem)target;
                    SinglePostView_obj17_Bindings bindings = new SinglePostView_obj17_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(element17.DataContext);
                    element17.DataContextChanged += bindings.DataContextChangedHandler;
                    global::Windows.UI.Xaml.DataTemplate.SetExtensionInstance(element17, bindings);
                    global::Windows.UI.Xaml.Markup.XamlBindingHelper.SetDataTemplateComponent(element17, bindings);
                }
                break;
            }
            return returnValue;
        }
    }
}

