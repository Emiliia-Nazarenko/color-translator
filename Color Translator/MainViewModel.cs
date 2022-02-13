using Color_Translator.Commands;
using Color_Translator.Model;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;


namespace Color_Translator
{
	public class MainViewModel : BaseModel
	{
		private static readonly BrushConverter _brushConverter = new BrushConverter();
		public ObservableCollection<SolidColorBrush> PaltetteColors { get; } = new ObservableCollection<SolidColorBrush>();
		public string[] colorsFromFile;
		public ICommand SaveCommand { get; set; }
		public ICommand DeleteCommand { get; set; }
		public ICommand ChooseColorCommand { get; set; }
		public ICommand AddCommand { get; set; }
		public ICommand OpenCommand { get; set; }

		/// <summary>
		/// The selected color
		/// </summary>
		[AlsoNotifyFor(nameof(RgbColor), nameof(HtmlColor), nameof(CurrentColorBrush))]
		public Color CurrentColor { get; set; } = Colors.Red;

		/// <summary>
		/// RGB format
		/// </summary>
		public string RgbColor
		{
			get => $"{CurrentColor.R},{CurrentColor.G},{CurrentColor.B}";
			set
			{
				try
				{
					var rgbBytes = value.Split(',').Select(x => byte.Parse(x)).ToArray();
					CurrentColor = Color.FromRgb(rgbBytes[0], rgbBytes[1], rgbBytes[2]);
				}
				catch { } // User entered invalid color
			}
		}

		/// <summary>
		/// HTML format (hexadecimal)
		/// </summary>
		public string HtmlColor
		{
			get => CurrentColor.ToString().Remove(1, 2); //Remove alpha channel
			set
			{
				try
				{
					CurrentColor = OldColorToNewColor(System.Drawing.ColorTranslator.FromHtml(value.Trim()));
				}
				catch { } // User entered invalid color
			}
		}

		public SolidColorBrush CurrentColorBrush
		{
			get
			{
				return new SolidColorBrush(CurrentColor);
			}
			set
			{
				CurrentColor = value?.Color ?? Colors.Black;
			}
		}

		public MainViewModel()
		{
			ChooseColorCommand = new RelayCommand(ChooseColor);
			AddCommand = new RelayCommand(Add);
			DeleteCommand = new RelayCommand(Delete);
			OpenCommand = new RelayCommand(Open);
			SaveCommand = new RelayCommand(Save);
		}

		private void ChooseColor(object o)
		{
			var colorDialog = new ColorDialog();
			if (colorDialog.ShowDialog() == DialogResult.OK)
				CurrentColorBrush = new SolidColorBrush(OldColorToNewColor(colorDialog.Color));
		}

		private void Add(object o)
		{
			if (!PaltetteColors.Any(x => x.Color == CurrentColorBrush.Color))
				PaltetteColors.Add(CurrentColorBrush);
		}

		private void Delete(object o)
		{
			var toRemove = PaltetteColors.FirstOrDefault(x => x.Color == CurrentColorBrush.Color);
			if (toRemove != null)
			{
				PaltetteColors.Remove(toRemove);
				CurrentColorBrush = PaltetteColors.FirstOrDefault();
			}
		}

		private void Open(object o)
		{
			var openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				PaltetteColors.Clear();
				foreach (var colorString in File.ReadAllLines(openFileDialog.FileName))
					PaltetteColors.Add((SolidColorBrush)_brushConverter.ConvertFromString(colorString));
				CurrentColorBrush = PaltetteColors.FirstOrDefault();
			}
		}

		private void Save(object o)
		{
			if (PaltetteColors.Count != 0)
			{
				var saveFileDialog = new SaveFileDialog();
				saveFileDialog.Filter = "Text files (*.txt)|*.txt";
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
					File.WriteAllLines(saveFileDialog.FileName, PaltetteColors.Select(x => _brushConverter.ConvertToString(x)));
			}
		}

		/// <summary>
		/// Converts the old System.Drawing.Color to System.Windows.Media.Color
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		private static Color OldColorToNewColor(System.Drawing.Color color)
		{
			return Color.FromRgb(color.R, color.G, color.B);
		}
	}
}
