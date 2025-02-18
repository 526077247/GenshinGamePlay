#if UNITY_2019_4_OR_NEWER
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    internal class DebuggerAssetListViewer
    {
        private class ProviderTableData : DefaultTableData
        {
            public DebugProviderInfo ProviderInfo;
        }
        private class DependTableData : DefaultTableData
        {
            public DebugBundleInfo BundleInfo;
        }

        private VisualTreeAsset _visualAsset;
        private TemplateContainer _root;

        private TableView _providerTableView;
        private TableView _dependTableView;

        private DebugReport _debugReport;
        private List<ITableData> _sourceDatas;


        /// <summary>
        /// 初始化页面
        /// </summary>
        public void InitViewer()
        {
            // 加载布局文件		
            _visualAsset = UxmlLoader.LoadWindowUXML<DebuggerAssetListViewer>();
            if (_visualAsset == null)
                return;

            _root = _visualAsset.CloneTree();
            _root.style.flexGrow = 1f;

            // 资源列表
            _providerTableView = _root.Q<TableView>("TopTableView");
            _providerTableView.SelectionChangedEvent = OnProviderTableViewSelectionChanged;
            CreateAssetTableViewColumns();

            // 依赖列表
            _dependTableView = _root.Q<TableView>("BottomTableView");
            CreateDependTableViewColumns();

#if UNITY_2020_3_OR_NEWER
            var topGroup = _root.Q<VisualElement>("TopGroup");
            var bottomGroup = _root.Q<VisualElement>("BottomGroup");
            topGroup.style.minHeight = 100;
            bottomGroup.style.minHeight = 100f;
            PanelSplitView.SplitVerticalPanel(_root, topGroup, bottomGroup);
#endif
        }
        private void CreateAssetTableViewColumns()
        {
            // PackageName
            {
                var columnStyle = new ColumnStyle(200);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("PackageName", "Package Name", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _providerTableView.AddColumn(column);
            }

            // AssetPath
            {
                var columnStyle = new ColumnStyle(600, 500, 1000);
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                var column = new TableColumn("AssetPath", "Asset Path", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _providerTableView.AddColumn(column);
            }

            // SpawnScene
            {
                var columnStyle = new ColumnStyle(150);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("SpawnScene", "Spawn Scene", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _providerTableView.AddColumn(column);
            }

            // SpawnTime
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("SpawnTime", "Spawn Time", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _providerTableView.AddColumn(column);
            }

            // LoadingTime
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("LoadingTime", "Loading Time", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _providerTableView.AddColumn(column);
            }

            // RefCount
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("RefCount", "Ref Count", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _providerTableView.AddColumn(column);
            }

            // Status
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("Status", "Status", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    StyleColor textColor;
                    var providerTableData = data as ProviderTableData;
                    if(providerTableData.ProviderInfo.Status == EOperationStatus.Failed.ToString())
                        textColor = new StyleColor(Color.yellow);
                    else
                        textColor = new StyleColor(Color.white);

                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                    infoLabel.style.color = textColor;
                };
                _providerTableView.AddColumn(column);
            }
        }
        private void CreateDependTableViewColumns()
        {
            //DependBundles
            {
                var columnStyle = new ColumnStyle(600, 500, 1000);
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                var column = new TableColumn("DependBundles", "Depend Bundles", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _dependTableView.AddColumn(column);
            }

            // RefCount
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("RefCount", "Ref Count", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                };
                _dependTableView.AddColumn(column);
            }

            // Status
            {
                var columnStyle = new ColumnStyle(100);
                columnStyle.Stretchable = false;
                columnStyle.Searchable = false;
                columnStyle.Sortable = true;
                var column = new TableColumn("Status", "Status", columnStyle);
                column.MakeCell = () =>
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    return label;
                };
                column.BindCell = (VisualElement element, ITableData data, ITableCell cell) =>
                {
                    StyleColor textColor;
                    var dependTableData = data as DependTableData;
                    if (dependTableData.BundleInfo.Status == EOperationStatus.Failed)
                        textColor = new StyleColor(Color.yellow);
                    else
                        textColor = new StyleColor(Color.white);

                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                    infoLabel.style.color = textColor;
                };
                _dependTableView.AddColumn(column);
            }
        }

        /// <summary>
        /// 填充页面数据
        /// </summary>
        public void FillViewData(DebugReport debugReport)
        {
            _debugReport = debugReport;

            // 清空旧数据
            _providerTableView.ClearAll(false, true);
            _dependTableView.ClearAll(false, true);

            // 填充数据源
            _sourceDatas = new List<ITableData>(1000);
            foreach (var packageData in debugReport.PackageDatas)
            {
                foreach (var providerInfo in packageData.ProviderInfos)
                {
                    var rowData = new ProviderTableData();
                    rowData.ProviderInfo = providerInfo;
                    rowData.AddAssetPathCell("PackageName", packageData.PackageName);
                    rowData.AddStringValueCell("AssetPath", providerInfo.AssetPath);
                    rowData.AddStringValueCell("SpawnScene", providerInfo.SpawnScene);
                    rowData.AddStringValueCell("SpawnTime", providerInfo.SpawnTime);
                    rowData.AddLongValueCell("LoadingTime", providerInfo.LoadingTime);
                    rowData.AddLongValueCell("RefCount", providerInfo.RefCount);
                    rowData.AddStringValueCell("Status", providerInfo.Status.ToString());
                    _sourceDatas.Add(rowData);
                }
            }
            _providerTableView.itemsSource = _sourceDatas;

            // 重建视图
            RebuildView(null);
        }

        /// <summary>
        /// 清空页面
        /// </summary>
        public void ClearView()
        {
            _debugReport = null;
            _providerTableView.ClearAll(false, true);
            _dependTableView.ClearAll(false, true);
            RebuildView(null);
        }

        /// <summary>
        /// 重建视图
        /// </summary>
        public void RebuildView(string searchKeyWord)
        {
            // 搜索匹配
            DefaultSearchSystem.Search(_sourceDatas, searchKeyWord);

            // 重建视图
            _providerTableView.RebuildView();
        }

        /// <summary>
        /// 挂接到父类页面上
        /// </summary>
        public void AttachParent(VisualElement parent)
        {
            parent.Add(_root);
        }

        /// <summary>
        /// 从父类页面脱离开
        /// </summary>
        public void DetachParent()
        {
            _root.RemoveFromHierarchy();
        }

        private void OnProviderTableViewSelectionChanged(ITableData data)
        {
            var providerTableData = data as ProviderTableData;
            DebugProviderInfo providerInfo = providerTableData.ProviderInfo;

            // 填充依赖数据
            var sourceDatas = new List<ITableData>(providerInfo.DependBundleInfos.Count);
            foreach (var dependBundleInfo in providerInfo.DependBundleInfos)
            {
                var rowData = new DependTableData();
                rowData.BundleInfo = dependBundleInfo;
                rowData.AddStringValueCell("DependBundles", dependBundleInfo.BundleName);
                rowData.AddLongValueCell("RefCount", dependBundleInfo.RefCount);
                rowData.AddStringValueCell("Status", dependBundleInfo.Status.ToString());
                sourceDatas.Add(rowData);
            }
            _dependTableView.itemsSource = sourceDatas;
            _dependTableView.RebuildView();
        }
    }
}
#endif