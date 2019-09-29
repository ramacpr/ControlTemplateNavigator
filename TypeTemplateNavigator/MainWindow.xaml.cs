using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xaml;
using System.Xml;

namespace TypeTemplateNavigator
{
    public class CustomTreeItem
    {       
        public ObservableCollection<CustomTreeItem> Children { get; set; }
        public string ItemDescription { get; set; }
        public bool IsItemExpanded { get; set; } 

        public CustomTreeItem()
        {
            Children = new ObservableCollection<CustomTreeItem>(); 
        }        
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged 
    {
        string typeTemplate;
        public string TypeTemplate
        {
            get => typeTemplate;
            set { typeTemplate = value; OnPropertyChanged("TypeTemplate"); }
        }

        ObservableCollection<Type> typeList;
        public ObservableCollection<Type> TypeList
        {
            get => typeList;
            set { typeList = value; OnPropertyChanged("TypeList"); }
        }

        ObservableCollection<CustomTreeItem> theTree;
        public ObservableCollection<CustomTreeItem> TheTree
        {
            get => theTree;
            set { theTree = value;
                OnPropertyChanged("TheTree"); }
        }


        #region Property change handler 
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string strPropName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(strPropName));
        } 
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            TypeList = new ObservableCollection<Type>();
            TheTree = new ObservableCollection<CustomTreeItem>();

            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Type controlType = typeof(Control);

            Assembly assembly = Assembly.GetAssembly(typeof(Control));
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(controlType) &&
                    !type.IsAbstract &&
                    type.IsPublic)
                    TypeList.Add(type);
            }
        }

        private void TypesList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            visTree.Items.Clear();
            if (tempStack.Children.Count > 0)
                tempStack.Children.Clear(); 

            try
            {
                Type type = (Type)typesList.SelectedItem;

                ConstructorInfo info = type.GetConstructor(Type.EmptyTypes);
                Control control = (Control)info.Invoke(null);
                control.Loaded += OnControlLoaded;
                tempStack.Children.Add(control);
                
                ControlTemplate template = control.Template;
                XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
                StringBuilder sb = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(sb, settings);
                System.Windows.Markup.XamlWriter.Save(template, writer);
                TypeTemplate = sb.ToString();

            }
            catch(Exception err)
            {
                TypeTemplate = "ERROR!!!\n" + err.Message;
            }
        }

        void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            Control control = sender as Control;
            if (control == null)
                return;

            UpdateVisualTree(control);

        }

        void UpdateVisualTree(DependencyObject control)
        {
            visTree.Items.Clear();
            ProcessElement(control, null);
        }

        void ProcessElement(DependencyObject element, TreeViewItem parentNode)
        {
            TreeViewItem treeNode = new TreeViewItem();
            treeNode.Header = element.GetType().Name;
            treeNode.IsExpanded = false;

            if (parentNode == null)
                visTree.Items.Add(treeNode);
            else
                parentNode.Items.Add(treeNode);

            int childCout = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childCout; i++)
                ProcessElement(VisualTreeHelper.GetChild(element, i), treeNode);
        }
    }
}
