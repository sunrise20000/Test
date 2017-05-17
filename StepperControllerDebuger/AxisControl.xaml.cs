﻿using IrixiStepperControllerHelper;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StepperControllerDebuger
{
    /// <summary>
    /// AxisControl.xaml 的交互逻辑
    /// </summary>
    public partial class AxisControl : UserControl
    {
        public AxisControl()
        {
            InitializeComponent();
        }



        public AxisState AxisState
        {
            get { return (AxisState)GetValue(AxisStateProperty); }
            set { SetValue(AxisStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AxisState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AxisStateProperty =
            DependencyProperty.Register("AxisState", typeof(AxisState), typeof(AxisControl), new PropertyMetadata(null));




        public bool IsAbsMode
        {
            get { return (bool)GetValue(IsAbsModeProperty); }
            set { SetValue(IsAbsModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsAbsMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAbsModeProperty =
            DependencyProperty.Register("IsAbsMode", typeof(bool), typeof(AxisControl), new PropertyMetadata(true));




        public int Distance
        {
            get { return (int)GetValue(DistanceProperty); }
            set { SetValue(DistanceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Distance.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DistanceProperty =
            DependencyProperty.Register("Distance", typeof(int), typeof(AxisControl), new PropertyMetadata(0));



        public int Speed
        {
            get { return (int)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Speed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpeedProperty =
            DependencyProperty.Register("Speed", typeof(int), typeof(AxisControl), new PropertyMetadata(100));


        public ICommand MoveToCW
        {
            get { return (ICommand)GetValue(MoveToCWProperty); }
            set { SetValue(MoveToCWProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MoveToCW.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MoveToCWProperty =
            DependencyProperty.Register("MoveToCW", typeof(ICommand), typeof(AxisControl), new PropertyMetadata(null));




        public object MoveToCWParameters
        {
            get { return (object)GetValue(MoveToCWParametersProperty); }
            set { SetValue(MoveToCWParametersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MoveToCWParameters.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MoveToCWParametersProperty =
            DependencyProperty.Register("MoveToCWParameters", typeof(object), typeof(AxisControl), new PropertyMetadata(null));




        public ICommand MoveToCCW
        {
            get { return (ICommand)GetValue(MoveToCCWProperty); }
            set { SetValue(MoveToCCWProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MoveToCCW.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MoveToCCWProperty =
            DependencyProperty.Register("MoveToCCW", typeof(ICommand), typeof(AxisControl), new PropertyMetadata(null));



        public object MoveToCCWParameters
        {
            get { return (object)GetValue(MoveToCCWParametersProperty); }
            set { SetValue(MoveToCCWParametersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MoveToCCWParameters.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MoveToCCWParametersProperty =
            DependencyProperty.Register("MoveToCCWParameters", typeof(object), typeof(AxisControl), new PropertyMetadata(null));

        private void btnMoveCCW_Click(object sender, RoutedEventArgs e)
        {
            this.MoveToCCW.Execute(this.MoveToCCWParameters);
        }

        private void btnMoveCW_Click(object sender, RoutedEventArgs e)
        {
            this.MoveToCW.Execute(this.MoveToCWParameters);
        }
    }
}