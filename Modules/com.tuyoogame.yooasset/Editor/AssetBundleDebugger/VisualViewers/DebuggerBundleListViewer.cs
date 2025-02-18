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
    internal class DebuggerBundleListViewer
    {
        private class BundleTableData : DefaultTableData
        {
            public string PackageName;
            public DebugBundleInfo BundleInfo;
        }
        private class UsingTableData : DefaultTableData
        {
            public DebugProviderInfo ProviderInfo;
        }

        private VisualTreeAsset _visualAsset;
        private TemplateContainer _root;

        private TableView _bundleTableView;
        private TableView _usingTableView;

        private DebugReport _debugReport;
        private List<ITableData> _sourceDatas;

        /// <summary>
        /// 初始化页面
        /// </summary>
        public void InitViewer()
        {
            // 加载布局文件
            _visualAsset = UxmlLoader.LoadWindowUXML<DebuggerBundleListViewer>();
            if (_visualAsset == null)
                return;

            _root = _visualAsset.CloneTree();
            _root.style.flexGrow = 1f;

            // 资源包列表
            _bundleTableView = _root.Q<TableView>("TopTableView");
            _bundleTableView.SelectionChangedEvent = OnBundleTableViewSelectionChanged;
            CreateBundleTableViewColumns();

            // 使用列表
            _usingTableView = _root.Q<TableView>("BottomTableView");
            CreateUsingTableViewColumns();

#if UNITY_2020_3_OR_NEWER
            var topGroup = _root.Q<VisualElement>("TopGroup");
            var bottomGroup = _root.Q<VisualElement>("BottomGroup");
            topGroup.style.minHeight = 100;
            bottomGroup.style.minHeight = 100f;
            PanelSplitView.SplitVerticalPanel(_root, topGroup, bottomGroup);
#endif
        }
        private void CreateBundleTableViewColumns()
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
                _bundleTableView.AddColumn(column);
            }

            // BundleName
            {
                var columnStyle = new ColumnStyle(600, 500, 1000);
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                var column = new TableColumn("BundleName", "Bundle Name", columnStyle);
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
                _bundleTableView.AddColumn(column);
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
                _bundleTableView.AddColumn(column);
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
                    var bundleTableData = data as BundleTableData;
                    if (bundleTableData.BundleInfo.Status == EOperationStatus.Failed)
                        textColor = new StyleColor(Color.yellow);
                    else
                        textColor = new StyleColor(Color.white);

                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                    infoLabel.style.color = textColor;
                };
                _bundleTableView.AddColumn(column);
            }
        }
        private void CreateUsingTableViewColumns()
        {
            // UsingAssets
            {
                var columnStyle = new ColumnStyle(600, 500, 1000);
                columnStyle.Stretchable = true;
                columnStyle.Searchable = true;
                columnStyle.Sortable = true;
                var column = new TableColumn("UsingAssets", "Using Assets", columnStyle);
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
                _usingTableView.AddColumn(column);
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
                _usingTableView.AddColumn(column);
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
                _usingTableView.AddColumn(column);
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
                _usingTableView.AddColumn(column);
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
                    var usingTableData = data as UsingTableData;
                    if (usingTableData.ProviderInfo.Status == EOperationStatus.Failed.ToString())
                        textColor = new StyleColor(Color.yellow);
                    else
                        textColor = new StyleColor(Color.white);

                    var infoLabel = element as Label;
                    infoLabel.text = (string)cell.GetDisplayObject();
                    infoLabel.style.color = textColor;
                };
                _usingTableView.AddColumn(column);
            }
        }

        /// <summary>
        /// 填充页面数据
        /// </summary>
        public void FillViewData(DebugReport debugReport)
        {
            _debugReport = debugReport;

            // 清空旧数据
            _bundleTableView.ClearAll(false, true);
            _usingTableView.ClearAll(false, true);

            // 填充数据源
            _sourceDatas = new List<ITableData>(1000);
            foreach (var packageData in debugReport.PackageDatas)
            {
                var tempDic = new HashSet<string>();
                foreach (var providerInfo in packageData.ProviderInfos)
                {
                    foreach (var bundleInfo in providerInfo.DependBundleInfos)
                    {
                        if (tempDic.Contains(bundleInfo.BundleName) == false)
                        {
                            tempDic.Add(bundleInfo.BundleName);

                            var rowData = new BundleTableData();
                            rowData.PackageName = packageData.PackageName;
                            rowData.BundleInfo = bundleInfo;
                            rowData.AddAssetPathCell("PackageName", packageData.PackageName);
                            rowData.AddStringValueCell("BundleName", bundleInfo.BundleName);
                            rowData.AddLongValueCell("RefCount", bundleInfo.RefCount);
                            rowData.AddStringValueCell("Status", bundleInfo.Status.ToString());
                            _sourceDatas.Add(rowData);
                        }
                    }
                }
            }
            _bundleTableView.itemsSource = _sourceDatas;

            // 重建视图
            RebuildView(null);
        }

        /// <summary>
        /// 清空页面
        /// </summary>
        public void ClearView()
        {
            _debugReport = null;
            _bundleTableView.ClearAll(false, true);
            _bundleTableView.RebuildView();
            _usingTableView.ClearAll(false, true);
            _usingTableView.RebuildView();
        }

        /// <summary>
        /// 重建视图
        /// </summary>
        public void RebuildView(string searchKeyWord)
        {
            // 搜索匹配
            DefaultSearchSystem.Search(_sourceDatas, searchKeyWord);

            // 重建视图
            _bundleTableView.RebuildView();
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

        private void OnBundleTableViewSelectionChanged(ITableData data)
        {
            var bundleTableData = data as BundleTableData;

            // 填充依赖数据
            var sourceDatas = new List<ITableData>(1000);
            foreach (var packageData in _debugReport.PackageDatas)
            {
                if (packageData.PackageName != bundleTableData.PackageName)
                    continue;

                foreach (var providerInfo in packageData.ProviderInfos)
                {
                    foreach (var bundleInfo in providerInfo.DependBundleInfos)
                    {
                        if (bundleInfo.BundleName == bundleTableData.BundleInfo.BundleName)
                        {
                            var rowData = new UsingTableData();
                            rowData.ProviderInfo = providerInfo;
                            rowData.AddStringValueCell("UsingAssets", providerInfo.AssetPath);
                            rowData.AddStringValueCell("SpawnScene", providerInfo.SpawnScene);
                            rowData.AddStringValueCell("SpawnTime", providerInfo.SpawnTime);
                            rowData.AddLongValueCell("RefCount", providerInfo.RefCount);
                            rowData.AddStringValueCell("Status", providerInfo.Status);
                            sourceDatas.Add(rowData);
                            break;
                        }
                    }
                }
            }
            _usingTableView.itemsSource = sourceDatas;
            _usingTableView.RebuildView();
        }
    }
}
#endif