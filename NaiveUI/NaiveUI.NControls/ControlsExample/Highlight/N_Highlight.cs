using NaiveUI.NControls.Tools;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NaiveUI.NControls.ControlsExample;

/// <summary>
/// 文本关键词高亮控件
/// </summary>
public class N_TextHighlighter : Control {
    #region 依赖属性

    // 源文本
    public static readonly DependencyProperty SourceTextProperty =
        ElementBase.Property<N_TextHighlighter,string>(nameof(SourceTextProperty),string.Empty, OnSourceTextChanged);


    // 高亮关键词集合
    public static readonly DependencyProperty HighlightWordsProperty =
        ElementBase.Property<N_TextHighlighter, IEnumerable<string>>(nameof(HighlightWordsProperty), null, OnHighlightWordsChanged);


    // 高亮前景色
    public static readonly DependencyProperty HighlightForegroundProperty =
        ElementBase.Property<N_TextHighlighter, Brush>(nameof(HighlightForegroundProperty), new SolidColorBrush(Colors.Red));
      

    // 高亮背景色
    public static readonly DependencyProperty HighlightBackgroundProperty =
         ElementBase.Property<N_TextHighlighter, Brush>(nameof(HighlightBackgroundProperty), new SolidColorBrush(Color.FromArgb(64, 255, 255, 0)));
   

    // 高亮字体样式
    public static readonly DependencyProperty HighlightFontWeightProperty =
        ElementBase.Property<N_TextHighlighter, FontWeight>(nameof(HighlightFontWeightProperty), FontWeights.Bold);
   

    // 是否区分大小写
    public static readonly DependencyProperty IsCaseSensitiveProperty =
        ElementBase.Property<N_TextHighlighter, bool>(nameof(IsCaseSensitiveProperty), true, OnVisualStylePropertyChanged);
   

    // 是否全词匹配
    public static readonly DependencyProperty IsWholeWordProperty =
        ElementBase.Property<N_TextHighlighter, bool>(nameof(IsWholeWordProperty), false, OnVisualStylePropertyChanged);

    // 使用正则表达式匹配
    public static readonly DependencyProperty UseRegexProperty =
        ElementBase.Property<N_TextHighlighter, bool>(nameof(UseRegexProperty), false, OnVisualStylePropertyChanged);
    
    // 高亮文本样式
    public static readonly DependencyProperty HighlightStyleProperty =
        ElementBase.Property<N_TextHighlighter, Style>(nameof(HighlightStyleProperty), null, OnVisualStylePropertyChanged);


    #endregion

    #region 属性访问器

    public Style HighlightStyle
    {
        get { return (Style)GetValue(HighlightStyleProperty); }
        set { SetValue(HighlightStyleProperty, value); }
    }

    public string SourceText {
        get { return (string)GetValue(SourceTextProperty); }
        set { SetValue(SourceTextProperty, value); }
    }

    public IEnumerable<string> HighlightWords {
        get { return (IEnumerable<string>)GetValue(HighlightWordsProperty); }
        set { SetValue(HighlightWordsProperty, value); }
    }

    public Brush HighlightForeground {
        get { return (Brush)GetValue(HighlightForegroundProperty); }
        set { SetValue(HighlightForegroundProperty, value); }
    }

    public Brush HighlightBackground {
        get { return (Brush)GetValue(HighlightBackgroundProperty); }
        set { SetValue(HighlightBackgroundProperty, value); }
    }

    public FontWeight HighlightFontWeight {
        get { return (FontWeight)GetValue(HighlightFontWeightProperty); }
        set { SetValue(HighlightFontWeightProperty, value); }
    }

    public bool IsCaseSensitive {
        get { return (bool)GetValue(IsCaseSensitiveProperty); }
        set { SetValue(IsCaseSensitiveProperty, value); }
    }

    public bool IsWholeWord {
        get { return (bool)GetValue(IsWholeWordProperty); }
        set { SetValue(IsWholeWordProperty, value); }
    }

    public bool UseRegex {
        get { return (bool)GetValue(UseRegexProperty); }
        set { SetValue(UseRegexProperty, value); }
    }

    #endregion

    #region 私有字段

    private TextBlock _textBlock;

    #endregion

    #region 构造函数

    static N_TextHighlighter() {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(N_TextHighlighter),
            new FrameworkPropertyMetadata(typeof(N_TextHighlighter)));
    }

    public N_TextHighlighter() {
        Loaded += OnLoaded;
    }

    #endregion

    #region 事件处理

    private void OnLoaded(object sender, RoutedEventArgs e) {
        UpdateHighlightedText();
    }

    private static void OnSourceTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var highlighter = (N_TextHighlighter)d;
        highlighter.UpdateHighlightedText();
    }

    private static void OnHighlightWordsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var highlighter = (N_TextHighlighter)d;
        highlighter.UpdateHighlightedText();
    }

    private static void OnVisualStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var highlighter = (N_TextHighlighter)d;
        highlighter.UpdateHighlightedText();
    }

    #endregion

    #region 重写方法

    public override void OnApplyTemplate() {
        base.OnApplyTemplate();

        // 获取模板中的TextBlock
        _textBlock = GetTemplateChild("PART_TextBlock") as TextBlock;

        if (_textBlock != null)
        {
            UpdateHighlightedText();
        }
    }

    #endregion

    #region 私有方法

    private void UpdateHighlightedText() {
        if (_textBlock == null) return;

        if ( string.IsNullOrEmpty(SourceText) || HighlightWords == null)
        {
            _textBlock.Text = SourceText;
            return;
        }

        // 清空现有内容
        _textBlock.Inlines.Clear();

        // 处理文本高亮
        int startIndex = 0;
        string text = SourceText;

        while (startIndex < text.Length)
        {
            // 查找下一个高亮词的位置
            int matchIndex = FindNextHighlightMatch(text, startIndex);

            if (matchIndex == -1)
            {
                // 没有更多匹配，添加剩余文本
                _textBlock.Inlines.Add(new AccessText(){Text = text.Substring(startIndex)});
                break;
            }

            // 添加匹配前的文本
            if (matchIndex > startIndex)
            {
                _textBlock.Inlines.Add(new AccessText(){Text = text.Substring(startIndex, matchIndex - startIndex)});
            }

            // 查找匹配的关键词
            string matchWord = FindMatchingWord(text, matchIndex);
            if (!string.IsNullOrEmpty(matchWord))
            {
                // 创建高亮文本
                Run highlightRun = new Run(matchWord) {
                    Foreground = HighlightForeground,
                    Background = HighlightBackground,
                    FontWeight = HighlightFontWeight
                };
                
                if (HighlightStyle != null)
                {
                    highlightRun.Style = HighlightStyle;
                }
                else
                {
                    // 应用默认属性
                    highlightRun.Foreground = HighlightForeground;
                    highlightRun.Background = HighlightBackground;
                    highlightRun.FontWeight = HighlightFontWeight;
                }
                
                _textBlock.Inlines.Add(highlightRun);

                // 移动到匹配词的末尾
                startIndex = matchIndex + matchWord.Length;
            }
            else
            {
                // 没有找到匹配的关键词，移动到下一个位置
                startIndex++;
            }
        }
    }

    private int FindNextHighlightMatch(string text, int startIndex) {
        int minMatchIndex = -1;
        string bestMatch = null;

        foreach (string word in HighlightWords)
        {
            if (string.IsNullOrEmpty(word))
                continue;

            int matchIndex;
            if (UseRegex)
            {
                // 使用正则表达式匹配
                RegexOptions options = IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                Match match = Regex.Match(text, word, options, TimeSpan.FromMilliseconds(100));
                if (match.Success && match.Index >= startIndex)
                {
                    if (minMatchIndex == -1 || match.Index < minMatchIndex)
                    {
                        minMatchIndex = match.Index;
                        bestMatch = word;
                    }
                }
            }
            else
            {
                // 普通文本匹配
                int index = IsCaseSensitive
                    ? text.IndexOf(word, startIndex)
                    : text.IndexOf(word, startIndex, StringComparison.OrdinalIgnoreCase);

                if (index != -1)
                {
                    if (IsWholeWord)
                    {
                        // 全词匹配检查
                        if (IsWholeWordMatch(text, index, word))
                        {
                            if (minMatchIndex == -1 || index < minMatchIndex)
                            {
                                minMatchIndex = index;
                                bestMatch = word;
                            }
                        }
                    }
                    else
                    {
                        if (minMatchIndex == -1 || index < minMatchIndex)
                        {
                            minMatchIndex = index;
                            bestMatch = word;
                        }
                    }
                }
            }
        }

        return minMatchIndex;
    }

    private string FindMatchingWord(string text, int matchIndex) {
        foreach (string word in HighlightWords)
        {
            if (string.IsNullOrEmpty(word))
                continue;

            if (UseRegex)
            {
                RegexOptions options = IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                Match match = Regex.Match(text, word, options);
                if (match.Success && match.Index == matchIndex)
                {
                    return match.Value;
                }
            }
            else
            {
                int index = IsCaseSensitive
                    ? text.IndexOf(word, matchIndex)
                    : text.IndexOf(word, matchIndex, StringComparison.OrdinalIgnoreCase);

                if (index == matchIndex && (IsWholeWord ? IsWholeWordMatch(text, index, word) : true))
                {
                    return word;
                }
            }
        }

        return null;
    }

    private bool IsWholeWordMatch(string text, int index, string word) {
        // 检查是否是全词匹配（前后为非字母数字字符或字符串边界）
        bool startIsWordBoundary = index == 0 || !char.IsLetterOrDigit(text[index - 1]);
        bool endIsWordBoundary = index + word.Length >= text.Length ||
                                 !char.IsLetterOrDigit(text[index + word.Length]);

        return startIsWordBoundary && endIsWordBoundary;
    }

    #endregion
}