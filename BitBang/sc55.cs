using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitBang
{
    public class sc55
    {
        #region main
        public sc55(byte _cid = 16, byte _dc = 1, byte _ce = 2, byte _data = 3, byte _rst = 4, byte _clk = 5)
        {
            cyclesInDelay = _cid;
            LCD_DC_PIN = _dc;
            LCD_CE_PIN = _ce;
            LCD_DATA_PIN = _data;
            LCD_RST_PIN = _rst;
            LCD_CLK_PIN = _clk;
        }

        byte _x;
        byte _y;

        readonly byte cyclesInDelay;
        // Распиновка порта
        readonly byte LCD_DC_PIN;
        readonly byte LCD_CE_PIN;
        readonly byte LCD_DATA_PIN;
        readonly byte LCD_RST_PIN;
        readonly byte LCD_CLK_PIN;

        public byte LCD_DDR
        {
            get; private set;
        }

        public List<byte> _LCD_PORT = new List<byte> { 0 };

        private byte LCD_PORT
        {
            get
            {
                return _LCD_PORT[_LCD_PORT.Count - 1];
            }
            set
            {
                _LCD_PORT.Add(value);
            }
        }
        //public byte LCD_PORT;

        public void InvertDisplay()
        {
            for( int i = 0; i< LcdCache.Length; i++)
            LcdCache[i] = (byte)~LcdCache[i];

            // Сброс указателей границ в максимальное значение
            LoWaterMark = 0;
            HiWaterMark = LCD_CACHE_SIZE - 1;

            UpdateLcd = true;
        }

        public void ListClear()
        {
            byte _tmp = _LCD_PORT.Last();
            _LCD_PORT.Clear();
            _LCD_PORT.Add(_tmp);
        }

        private static byte _BV(byte pin)
        {
            return (byte)(1 << pin);
        }

        private void Delay()
        {
            for (int i = 0; i < cyclesInDelay; i++) LCD_PORT = LCD_PORT;
        }
        #endregion

        #region constant

        // Порт к которому подключен LCD (здесь пример распиновки для ATmega8)
        //const byte LCD_PORT = 0; // PORTB
        //const byte LCD_DDR = 0;  // DDRB

        // Разрешение дисплея в пикселях
        const byte LCD_X_RES = 102;   // разрешение по горизонтали
        const byte LCD_Y_RES = 65;    // разрешение по вертикали
        const byte LCD_LINES = (LCD_Y_RES / 8);    // количество строк
        const byte LCD_COLS = (LCD_X_RES / 5);    // количество столбцов

        // Настройки для рисования группы прямоугольников функцией LcdBars ( byte data[], byte numbBars, byte width, byte multiplier )
        const byte EMPTY_SPACE_BARS = 2;    // расстояние между прямоугольниками
        const byte BAR_X = 30;              // координата x
        const byte BAR_Y = 47;              // координата y

        // Размер кэша ( 84 * 48 ) / 8 = 504 байта
        const int LCD_CACHE_SIZE = ((LCD_X_RES * LCD_Y_RES) / 8);

        // Для возвращаемых значений
        const byte OK = 0;                  // Безошибочная отрисовка
        const byte OUT_OF_BORDER = 1;       // Выход за границы дисплея
        const byte OK_WITH_WRAP = 2;        // Переход на начало (ситуация автоинкремента указателя курсора при выводе длинного текста)

        // Перечисления

        const byte LCD_CMD = 0;     // Команда
        const byte LCD_DATA = 1;    // Данные

        const byte PIXEL_OFF = 0;   // Погасить пиксели дисплея
        const byte PIXEL_ON = 1;   // Включить пиксели дисплея
        const byte PIXEL_XOR = 2;   // Инвертировать пиксели

        const byte FONT_1X = 1;      // Обычный размер шрифта 5x7
        const byte FONT_2X = 2;       // Увеличенный размер шрифта


        /*
        // Прототипы функций, детальную информацию смотрим внутри n3310lcd.c
        void LcdInit(void );   // Инициализация
        void LcdClear(void );   // Очистка буфера
        void LcdUpdate(void );   // Копирование буфера в ОЗУ дисплея
        void LcdImage( const byte* imageData );   // Рисование картинки из массива в Flash ROM
        void LcdContrast(byte contrast);   // Установка контрастности дисплея
        byte LcdGotoXYFont(byte x, byte y);   // Установка курсора в позицию x,y
        byte LcdChr(LcdFontSize size, byte ch);   // Вывод символа в текущей позиции
        byte LcdStr(LcdFontSize size, byte dataArray[] );   // Вывод строки сохраненной в RAM
        byte LcdFStr(LcdFontSize size, const byte* dataPtr );   // Вывод строки сохраненной в Flash ROM
        byte LcdPixel(byte x, byte y, LcdPixelMode mode);   // Точка
        byte LcdLine(byte x1, byte y1, byte x2, byte y2, LcdPixelMode mode);   // Линия
        byte LcdCircle(byte x, byte y, byte radius, LcdPixelMode mode);   // Окружность
        byte LcdRect(byte x1, byte y1, byte x2, byte y2, LcdPixelMode mode);   // Прямоугольник
        byte LcdSingleBar(byte baseX, byte baseY, byte height, byte width, LcdPixelMode mode);   // Один
        byte LcdBars(byte data[], byte numbBars, byte width, byte multiplier);   // Несколько
        */



        /*
         * Таблица для отображения символов (ASCII[0x20-0x7F] + CP1251[0xC0-0xFF] = всего 160 символов)
         */
        byte[,] FontLookup ={
            { 0x00, 0x00, 0x00, 0x00, 0x00 },   //   0x20  32
            { 0x00, 0x00, 0x5F, 0x00, 0x00 },   // ! 0x21  33
            { 0x00, 0x07, 0x00, 0x07, 0x00 },   // " 0x22  34
            { 0x14, 0x7F, 0x14, 0x7F, 0x14 },   // # 0x23  35
            { 0x24, 0x2A, 0x7F, 0x2A, 0x12 },   // $ 0x24  36
            { 0x4C, 0x2C, 0x10, 0x68, 0x64 },   // % 0x25  37
            { 0x36, 0x49, 0x55, 0x22, 0x50 },   // & 0x26  38
            { 0x00, 0x05, 0x03, 0x00, 0x00 },   // ' 0x27  39
            { 0x00, 0x1C, 0x22, 0x41, 0x00 },   // ( 0x28  40
            { 0x00, 0x41, 0x22, 0x1C, 0x00 },   // ) 0x29  41
            { 0x14, 0x08, 0x3E, 0x08, 0x14 },   // * 0x2A  42
            { 0x08, 0x08, 0x3E, 0x08, 0x08 },   // + 0x2B  43
            { 0x00, 0x00, 0x50, 0x30, 0x00 },   // , 0x2C  44
            { 0x10, 0x10, 0x10, 0x10, 0x10 },   // - 0x2D  45
            { 0x00, 0x60, 0x60, 0x00, 0x00 },   // . 0x2E  46
            { 0x20, 0x10, 0x08, 0x04, 0x02 },   // / 0x2F  47
            { 0x3E, 0x51, 0x49, 0x45, 0x3E },   // 0 0x30  48
            { 0x00, 0x42, 0x7F, 0x40, 0x00 },   // 1 0x31  49
            { 0x42, 0x61, 0x51, 0x49, 0x46 },   // 2 0x32  50
            { 0x21, 0x41, 0x45, 0x4B, 0x31 },   // 3 0x33  51
            { 0x18, 0x14, 0x12, 0x7F, 0x10 },   // 4 0x34  52
            { 0x27, 0x45, 0x45, 0x45, 0x39 },   // 5 0x35  53
            { 0x3C, 0x4A, 0x49, 0x49, 0x30 },   // 6 0x36  54
            { 0x01, 0x71, 0x09, 0x05, 0x03 },   // 7 0x37  55
            { 0x36, 0x49, 0x49, 0x49, 0x36 },   // 8 0x38  56
            { 0x06, 0x49, 0x49, 0x29, 0x1E },   // 9 0x39  57
            { 0x00, 0x36, 0x36, 0x00, 0x00 },   // : 0x3A  58
            { 0x00, 0x56, 0x36, 0x00, 0x00 },   // ; 0x3B  59
            { 0x08, 0x14, 0x22, 0x41, 0x00 },   // < 0x3C  60
            { 0x14, 0x14, 0x14, 0x14, 0x14 },   // = 0x3D  61
            { 0x00, 0x41, 0x22, 0x14, 0x08 },   // > 0x3E  62
            { 0x02, 0x01, 0x51, 0x09, 0x06 },   // ? 0x3F  63
            { 0x32, 0x49, 0x79, 0x41, 0x3E },   // @ 0x40  64
            { 0x7E, 0x11, 0x11, 0x11, 0x7E },   // A 0x41  65
            { 0x7F, 0x49, 0x49, 0x49, 0x36 },   // B 0x42  66
            { 0x3E, 0x41, 0x41, 0x41, 0x22 },   // C 0x43  67
            { 0x7F, 0x41, 0x41, 0x22, 0x1C },   // D 0x44  68
            { 0x7F, 0x49, 0x49, 0x49, 0x41 },   // E 0x45  69
            { 0x7F, 0x09, 0x09, 0x09, 0x01 },   // F 0x46  70
            { 0x3E, 0x41, 0x49, 0x49, 0x7A },   // G 0x47  71
            { 0x7F, 0x08, 0x08, 0x08, 0x7F },   // H 0x48  72
            { 0x00, 0x41, 0x7F, 0x41, 0x00 },   // I 0x49  73
            { 0x20, 0x40, 0x41, 0x3F, 0x01 },   // J 0x4A  74
            { 0x7F, 0x08, 0x14, 0x22, 0x41 },   // K 0x4B  75
            { 0x7F, 0x40, 0x40, 0x40, 0x40 },   // L 0x4C  76
            { 0x7F, 0x02, 0x0C, 0x02, 0x7F },   // M 0x4D  77
            { 0x7F, 0x04, 0x08, 0x10, 0x7F },   // N 0x4E  78
            { 0x3E, 0x41, 0x41, 0x41, 0x3E },   // O 0x4F  79
            { 0x7F, 0x09, 0x09, 0x09, 0x06 },   // P 0x50  80
            { 0x3E, 0x41, 0x51, 0x21, 0x5E },   // Q 0x51  81
            { 0x7F, 0x09, 0x19, 0x29, 0x46 },   // R 0x52  82
            { 0x46, 0x49, 0x49, 0x49, 0x31 },   // S 0x53  83
            { 0x01, 0x01, 0x7F, 0x01, 0x01 },   // T 0x54  84
            { 0x3F, 0x40, 0x40, 0x40, 0x3F },   // U 0x55  85
            { 0x1F, 0x20, 0x40, 0x20, 0x1F },   // V 0x56  86
            { 0x3F, 0x40, 0x38, 0x40, 0x3F },   // W 0x57  87
            { 0x63, 0x14, 0x08, 0x14, 0x63 },   // X 0x58  88
            { 0x07, 0x08, 0x70, 0x08, 0x07 },   // Y 0x59  89
            { 0x61, 0x51, 0x49, 0x45, 0x43 },   // Z 0x5A  90
            { 0x00, 0x7F, 0x41, 0x41, 0x00 },   // [ 0x5B  91
            { 0x02, 0x04, 0x08, 0x10, 0x20 },   // \ 0x5C  92
            { 0x00, 0x41, 0x41, 0x7F, 0x00 },   // ] 0x5D  93
            { 0x04, 0x02, 0x01, 0x02, 0x04 },   // ^ 0x5E  94
            { 0x40, 0x40, 0x40, 0x40, 0x40 },   // _ 0x5F  95
            { 0x00, 0x01, 0x02, 0x04, 0x00 },   // ` 0x60  96
            { 0x20, 0x54, 0x54, 0x54, 0x78 },   // a 0x61  97
            { 0x7F, 0x48, 0x44, 0x44, 0x38 },   // b 0x62  98
            { 0x38, 0x44, 0x44, 0x44, 0x20 },   // c 0x63  99
            { 0x38, 0x44, 0x44, 0x48, 0x7F },   // d 0x64 100
            { 0x38, 0x54, 0x54, 0x54, 0x18 },   // e 0x65 101
            { 0x08, 0x7E, 0x09, 0x01, 0x02 },   // f 0x66 102
            { 0x0C, 0x52, 0x52, 0x52, 0x3E },   // g 0x67 103
            { 0x7F, 0x08, 0x04, 0x04, 0x78 },   // h 0x68 104
            { 0x00, 0x44, 0x7D, 0x40, 0x00 },   // i 0x69 105
            { 0x20, 0x40, 0x44, 0x3D, 0x00 },   // j 0x6A 106
            { 0x7F, 0x10, 0x28, 0x44, 0x00 },   // k 0x6B 107
            { 0x00, 0x41, 0x7F, 0x40, 0x00 },   // l 0x6C 108
            { 0x7C, 0x04, 0x18, 0x04, 0x78 },   // m 0x6D 109
            { 0x7C, 0x08, 0x04, 0x04, 0x78 },   // n 0x6E 110
            { 0x38, 0x44, 0x44, 0x44, 0x38 },   // o 0x6F 111
            { 0x7C, 0x14, 0x14, 0x14, 0x08 },   // p 0x70 112
            { 0x08, 0x14, 0x14, 0x18, 0x7C },   // q 0x71 113
            { 0x7C, 0x08, 0x04, 0x04, 0x08 },   // r 0x72 114
            { 0x48, 0x54, 0x54, 0x54, 0x20 },   // s 0x73 115
            { 0x04, 0x3F, 0x44, 0x40, 0x20 },   // t 0x74 116
            { 0x3C, 0x40, 0x40, 0x20, 0x7C },   // u 0x75 117
            { 0x1C, 0x20, 0x40, 0x20, 0x1C },   // v 0x76 118
            { 0x3C, 0x40, 0x30, 0x40, 0x3C },   // w 0x77 119
            { 0x44, 0x28, 0x10, 0x28, 0x44 },   // x 0x78 120
            { 0x0C, 0x50, 0x50, 0x50, 0x3C },   // y 0x79 121
            { 0x44, 0x64, 0x54, 0x4C, 0x44 },   // z 0x7A 122
            { 0x00, 0x08, 0x36, 0x41, 0x00 },   // { 0x7B 123
            { 0x00, 0x00, 0x7F, 0x00, 0x00 },   // | 0x7C 124
            { 0x00, 0x41, 0x36, 0x08, 0x00 },   // } 0x7D 125
            { 0x08, 0x04, 0x08, 0x10, 0x08 },   // ~ 0x7E 126
            { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },   //  0x7F 127
            
            { 0x7C, 0x12, 0x11, 0x12, 0x7C },   // А 0xC0 192
            { 0x7F, 0x49, 0x49, 0x49, 0x31 },   // Б 0xC1 193
            { 0x7F, 0x49, 0x49, 0x49, 0x36 },   // В 0xC2 194
            { 0x7F, 0x01, 0x01, 0x01, 0x01 },   // Г 0xC3 195
            { 0x60, 0x3F, 0x21, 0x3F, 0x60 },   // Д 0xC4 196
            { 0x7F, 0x49, 0x49, 0x49, 0x41 },   // Е 0xC5 197
            { 0x77, 0x08, 0x7F, 0x08, 0x77 },   // Ж 0xC6 198
            { 0x22, 0x41, 0x49, 0x49, 0x36 },   // З 0xC7 199
            { 0x7F, 0x10, 0x08, 0x04, 0x7F },   // И 0xC8 200
            { 0x7E, 0x10, 0x09, 0x04, 0x7E },   // Й 0xC9 201
            { 0x7F, 0x08, 0x14, 0x22, 0x41 },   // К 0xCA 202
            { 0x40, 0x3E, 0x01, 0x01, 0x7F },   // Л 0xCB 203
            { 0x7F, 0x02, 0x0C, 0x02, 0x7F },   // М 0xCC 204
            { 0x7F, 0x08, 0x08, 0x08, 0x7F },   // Н 0xCD 205
            { 0x3E, 0x41, 0x41, 0x41, 0x3E },   // О 0xCE 206
            { 0x7F, 0x01, 0x01, 0x01, 0x7F },   // П 0xCF 207
            { 0x7F, 0x09, 0x09, 0x09, 0x06 },   // Р 0xD0 208
            { 0x3E, 0x41, 0x41, 0x41, 0x22 },   // С 0xD1 209
            { 0x01, 0x01, 0x7F, 0x01, 0x01 },   // Т 0xD2 210
            { 0x07, 0x48, 0x48, 0x48, 0x3F },   // У 0xD3 211
            { 0x0E, 0x11, 0x7F, 0x11, 0x0E },   // Ф 0xD4 212
            { 0x63, 0x14, 0x08, 0x14, 0x63 },   // Х 0xD5 213
            { 0x3F, 0x20, 0x20, 0x3F, 0x60 },   // Ц 0xD6 214
            { 0x07, 0x08, 0x08, 0x08, 0x7F },   // Ч 0xD7 215
            { 0x7F, 0x40, 0x7E, 0x40, 0x7F },   // Ш 0xD8 216
            { 0x3F, 0x20, 0x3F, 0x20, 0x7F },   // Щ 0xD9 217
            { 0x01, 0x7F, 0x48, 0x48, 0x30 },   // Ъ 0xDA 218
            { 0x7F, 0x48, 0x30, 0x00, 0x7F },   // Ы 0xDB 219
            { 0x00, 0x7F, 0x48, 0x48, 0x30 },   // Ь 0xDC 220
            { 0x22, 0x41, 0x49, 0x49, 0x3E },   // Э 0xDD 221
            { 0x7F, 0x08, 0x3E, 0x41, 0x3E },   // Ю 0xDE 222
            { 0x46, 0x29, 0x19, 0x09, 0x7F },   // Я 0xDF 223
            { 0x20, 0x54, 0x54, 0x54, 0x78 },   // а 0xE0 224
            { 0x3C, 0x4A, 0x4A, 0x4A, 0x31 },   // б 0xE1 225
            { 0x7C, 0x54, 0x54, 0x28, 0x00 },   // в 0xE2 226
            { 0x7C, 0x04, 0x04, 0x0C, 0x00 },   // г 0xE3 227
            { 0x60, 0x3C, 0x24, 0x3C, 0x60 },   // д 0xE4 228
            { 0x38, 0x54, 0x54, 0x54, 0x18 },   // е 0xE5 229
            { 0x6C, 0x10, 0x7C, 0x10, 0x6C },   // ж 0xE6 230
            { 0x00, 0x44, 0x54, 0x54, 0x28 },   // з 0xE7 231
            { 0x7C, 0x20, 0x10, 0x08, 0x7C },   // и 0xE8 232
            { 0x7C, 0x21, 0x12, 0x09, 0x7C },   // й 0xE9 233
            { 0x7C, 0x10, 0x28, 0x44, 0x00 },   // к 0xEA 234
            { 0x40, 0x38, 0x04, 0x04, 0x7C },   // л 0xEB 235
            { 0x7C, 0x08, 0x10, 0x08, 0x7C },   // м 0xEC 236
            { 0x7C, 0x10, 0x10, 0x10, 0x7C },   // н 0xED 237
            { 0x38, 0x44, 0x44, 0x44, 0x38 },   // о 0xEE 238
            { 0x7C, 0x04, 0x04, 0x04, 0x7C },   // п 0xEF 239
            { 0x7C, 0x14, 0x14, 0x14, 0x08 },   // р 0xF0 240
            { 0x38, 0x44, 0x44, 0x44, 0x00 },   // с 0xF1 241
            { 0x04, 0x04, 0x7C, 0x04, 0x04 },   // т 0xF2 242
            { 0x0C, 0x50, 0x50, 0x50, 0x3C },   // у 0xF3 243
            { 0x08, 0x14, 0x7C, 0x14, 0x08 },   // ф 0xF4 244
            { 0x44, 0x28, 0x10, 0x28, 0x44 },   // х 0xF5 245
            { 0x3C, 0x20, 0x20, 0x3C, 0x60 },   // ц 0xF6 246
            { 0x0C, 0x10, 0x10, 0x10, 0x7C },   // ч 0xF7 247
            { 0x7C, 0x40, 0x7C, 0x40, 0x7C },   // ш 0xF8 248
            { 0x3C, 0x20, 0x3C, 0x20, 0x7C },   // щ 0xF9 249
            { 0x04, 0x7C, 0x50, 0x50, 0x20 },   // ъ 0xFA 250
            { 0x7C, 0x50, 0x20, 0x00, 0x7C },   // ы 0xFB 251
            { 0x00, 0x7C, 0x50, 0x50, 0x20 },   // ь 0xFC 252
            { 0x28, 0x44, 0x54, 0x54, 0x38 },   // э 0xFD 253
            { 0x7C, 0x10, 0x38, 0x44, 0x38 },   // ю 0xFE 254
            { 0x48, 0x54, 0x34, 0x14, 0x7C }    // я 0xFF 255
};
        #endregion

        #region static

        // Прототипы приватных функций драйвера

        //static void LcdSend(byte data, LcdCmdData cd);
        //static void Delay(void );

        // Глобальные переменные

        // Кэш в ОЗУ 84*48 бит или 504 байта
        static byte[] LcdCache = new byte[LCD_CACHE_SIZE];

        // Чтобы не обновлять весь дисплей, а лишь ту часть что изменилась,
        // будем отмечать две границы кэша где произошли изменения. Затем
        // можно копировать эту часть кэша между границами в ОЗУ дисплея.
        static int LoWaterMark;   // нижняя граница
        static int HiWaterMark;   // верхняя граница

        // Указатель для работы с LcdCache[]
        static int LcdCacheIdx;

        // Флаг изменений кэша
        static bool UpdateLcd = false;

        #endregion

        #region original

        public void LcdInit()
        {
            // Pull-up на вывод подключенный к reset дисплея
            LCD_PORT |= _BV(LCD_RST_PIN);

            // Устанавливаем нужные биты порта на выход
            LCD_DDR |= (byte)(_BV(LCD_RST_PIN) | _BV(LCD_DC_PIN) | _BV(LCD_CE_PIN) | _BV(LCD_DATA_PIN) | _BV(LCD_CLK_PIN));

            // Некалиброванная задержка
            Delay();

            // Дергаем reset
            LCD_PORT &= (byte)~(_BV(LCD_RST_PIN));
            Delay();
            LCD_PORT |= _BV(LCD_RST_PIN);

            // Отключаем LCD контроллер - высокий уровень на SCE
            LCD_PORT |= _BV(LCD_CE_PIN);

            // Отправляем команды дисплею
            LcdSend(0x21, LCD_CMD); // Включаем расширенный набор команд (LCD Extended Commands)
            LcdSend(0xC8, LCD_CMD); // Установка контрастности (LCD Vop)
            LcdSend(0x06, LCD_CMD); // Установка температурного коэффициента (Temp coefficent)
            LcdSend(0x16, LCD_CMD); // Настройка питания (Bias n=2), siemens
            LcdSend(0x20, LCD_CMD); // Включаем стандартный набор команд и горизонтальную адресацию (LCD Standard Commands,Horizontal addressing mode)
            LcdSend(0x0C, LCD_CMD); // Нормальный режим (LCD in normal mode)

            // Первичная очистка дисплея
            LcdClear();
            LcdUpdate();
        }



        /*
         * Имя                   :  LcdClear
         * Описание              :  Очищает дисплей. Далее необходимо выполнить LcdUpdate
         * Аргумент(ы)           :  Нет
         * Возвращаемое значение :  Нет
         */
        public void LcdClear()
        {
            //    // Очистка кэша дисплея
            //    int i;
            //    for ( i = 0; i < LCD_CACHE_SIZE; i++ )
            //    {
            //        LcdCache[i] = 0x00;
            //    }

            // Оптимизация от Jakub Lasinski (March 14 2009)
            //memset(LcdCache, 0x00, LCD_CACHE_SIZE);
            for (int i = 0; i < LCD_CACHE_SIZE; i++) LcdCache[i] = 0x00;

            // Сброс указателей границ в максимальное значение
            LoWaterMark = 0;
            HiWaterMark = LCD_CACHE_SIZE - 1;

            // Установка флага изменений кэша
            UpdateLcd = true;
        }



        /*
         * Имя                   :  LcdUpdate
         * Описание              :  Копирует кэш в ОЗУ дисплея
         * Аргумент(ы)           :  Нет
         * Возвращаемое значение :  Нет
         */
        public void LcdUpdate()
        {
            if (UpdateLcd)
            {
                int i;

                if (LoWaterMark < 0)
                    LoWaterMark = 0;
                else if (LoWaterMark >= LCD_CACHE_SIZE)
                    LoWaterMark = LCD_CACHE_SIZE - 1;

                if (HiWaterMark < 0)
                    HiWaterMark = 0;
                else if (HiWaterMark >= LCD_CACHE_SIZE)
                    HiWaterMark = LCD_CACHE_SIZE - 1;

                // Устанавливаем начальный адрес в соответствии к LoWaterMark
                LcdSend((byte)(0x80 | (LoWaterMark % LCD_X_RES)), LCD_CMD);
                LcdSend((byte)(0x40 | (LoWaterMark / LCD_X_RES)), LCD_CMD);

                // Обновляем необходимую часть буфера дисплея
                for (i = LoWaterMark; i <= HiWaterMark; i++)
                {
                    // Для оригинального дисплея не нужно следить за адресом в буфере,
                    // можно просто последовательно выводить данные
                    LcdSend(LcdCache[i], LCD_DATA);
                }

                // Сброс указателей границ в пустоту
                LoWaterMark = LCD_CACHE_SIZE - 1;
                HiWaterMark = 0;

                // Сброс флага изменений кэша
                UpdateLcd = false;
            }
        }


        /*
         * Имя                   :  LcdSend
         * Описание              :  Отправляет данные в контроллер дисплея
         * Аргумент(ы)           :  data -> данные для отправки
         *                          cd   -> команда или данные (смотри enum в n3310.h)
         * Возвращаемое значение :  Нет
         */
        private void LcdSend(byte data, byte cd)
        {
            // Включаем контроллер дисплея (низкий уровень активный)
            LCD_PORT &= (byte)~(_BV(LCD_CE_PIN));

            byte i;

            if (cd == LCD_DATA)
                LCD_PORT |= _BV(LCD_DC_PIN);
            else
                LCD_PORT &= (byte)~_BV(LCD_DC_PIN);

            for (i = 0; i < 8; i++)
            {

                if (((data >> (7 - i)) & 1) > 0)
                {
                    LCD_PORT |= _BV(LCD_DATA_PIN);
                }
                else
                {
                    LCD_PORT &= (byte)~_BV(LCD_DATA_PIN);
                }

                LCD_PORT |= _BV(LCD_CLK_PIN);
                LCD_PORT &= (byte)~_BV(LCD_CLK_PIN);

            }
            LCD_PORT |= _BV(LCD_DATA_PIN);
            LCD_PORT |= _BV(LCD_DC_PIN);

            // Отключаем контроллер дисплея
            LCD_PORT |= _BV(LCD_CE_PIN);
        }



        /*
         * Имя                   :  LcdContrast
         * Описание              :  Устанавливает контрастность дисплея
         * Аргумент(ы)           :  контраст -> значение от 0x00 к 0x7F
         * Возвращаемое значение :  Нет
         */
        public void LcdContrast(byte contrast)
        {
            LcdSend(0x21, LCD_CMD);              // Расширенный набор команд
            LcdSend((byte)(0x80 | contrast), LCD_CMD);   // Установка уровня контрастности
            LcdSend(0x20, LCD_CMD);              // Стандартный набор команд, горизонтальная адресация
        }



        /*
         * Имя                   :  Delay
         * Описание              :  Некалиброванная задержка для процедуры инициализации LCD
         * Аргумент(ы)           :  Нет
         * Возвращаемое значение :  Нет
         */
        /*static void Delay()
        {
            int i;

            for (i = -32000; i < 32000; i++) ;
        }
        */


        /*
         * Имя                   :  LcdGotoXYFont
         * Описание              :  Устанавливает курсор в позицию x,y относительно стандартного размера шрифта
         * Аргумент(ы)           :  x,y -> координаты новой позиции курсора. Значения: 0,0 .. 13,5
         * Возвращаемое значение :  смотри возвращаемое значение в n3310.h
         */
        public byte LcdGotoXYFont(byte x, byte y)
        {
            // Проверка границ
            if (x > LCD_COLS || y > LCD_LINES) return OUT_OF_BORDER;
            _x = x;
            _y = y;
            //  Вычисление указателя. Определен как адрес в пределах 504 байт
            LcdCacheIdx = x * LCD_LINES + y * LCD_X_RES;
            return OK;
        }



        /*
         * Имя                   :  LcdChr
         * Описание              :  Выводит символ в текущей позиции курсора, затем инкрементирует положение курсора
         * Аргумент(ы)           :  size -> размер шрифта. Смотри enum в n3310.h
         *                          ch   -> символ для вывода
         * Возвращаемое значение :  смотри возвращаемое значение в n3310lcd.h
         */
        private byte LcdChr(byte size, uint ch)
        {
            byte i, c;
            byte b1, b2;
            int tmpIdx;

            if (LcdCacheIdx < LoWaterMark)
            {
                // Обновляем нижнюю границу
                LoWaterMark = LcdCacheIdx;
            }
            if ((ch == 10) || (ch == 13))
            {
                if (ch == 10)
                {
                    LcdGotoXYFont(0, (byte)(_y + size));
                }
                return 0;
            }
            if ((ch >= 0x20) && (ch <= 0x7F))
            {
                // Смещение в таблице для символов ASCII[0x20-0x7F]
                ch -= 32;
            }
            else if (ch >= 0xC0)
            {
                // Смещение в таблице для символов CP1251[0xC0-0xFF]
                ch += 96;
                ch -= 1040;
            }
            else
            {
                // Остальные игнорируем (их просто нет в таблице для экономии памяти)
                ch = 95;
            }

            if (size == FONT_1X)
            {
                for (i = 0; i < 5; i++)
                {
                    // Копируем вид символа из таблицы в кэш
                    //LcdCache[LcdCacheIdx++] = pgm_read_byte(&(FontLookup[ch][i])) << 1;
                    LcdCache[LcdCacheIdx++] = (byte)(FontLookup[ch, i] << 1);
                }
            }
            else if (size == FONT_2X)
            {
                tmpIdx = LcdCacheIdx - LCD_X_RES;

                if (tmpIdx < LoWaterMark)
                {
                    LoWaterMark = tmpIdx;
                }

                if (tmpIdx < 0) return OUT_OF_BORDER;

                for (i = 0; i < 5; i++)
                {
                    // Копируем вид символа из таблицы у временную переменную
                    //c = pgm_read_byte(&(FontLookup[ch][i])) << 1;
                    c = (byte)(FontLookup[ch, i] << 1);
                    // Увеличиваем картинку
                    // Первую часть
                    b1 = (byte)((c & 0x01) * 3);
                    b1 |= (byte)((c & 0x02) * 6);
                    b1 |= (byte)((c & 0x04) * 12);
                    b1 |= (byte)((c & 0x08) * 24);

                    c >>= 4;
                    // Вторую часть
                    b2 = (byte)((c & 0x01) * 3);
                    b2 |= (byte)((c & 0x02) * 6);
                    b2 |= (byte)((c & 0x04) * 12);
                    b2 |= (byte)((c & 0x08) * 24);

                    // Копируем две части в кэш
                    LcdCache[tmpIdx++] = b1;
                    LcdCache[tmpIdx++] = b1;
                    LcdCache[tmpIdx + LCD_X_RES - 2] = b2;
                    LcdCache[tmpIdx + LCD_X_RES - 1] = b2;
                }

                // Обновляем x координату курсора
                LcdCacheIdx = (LcdCacheIdx + 11) % LCD_CACHE_SIZE;
            }

            if (LcdCacheIdx > HiWaterMark)
            {
                // Обновляем верхнюю границу
                HiWaterMark = LcdCacheIdx;
            }

            // Горизонтальный разрыв между символами
            LcdCache[LcdCacheIdx] = 0x00;
            // Если достигли позицию указателя LCD_CACHE_SIZE - 1, переходим в начало
            if (LcdCacheIdx == (LCD_CACHE_SIZE - 1))
            {
                LcdCacheIdx = 0;
                return OK_WITH_WRAP;
            }
            // Иначе просто инкрементируем указатель
            LcdCacheIdx++;
            return OK;
        }



        /*
         * Имя                   :  LcdStr
         * Описание              :  Эта функция предназначена для печати строки которая хранится в RAM
         * Аргумент(ы)           :  size      -> размер шрифта. Смотри enum в n3310.h
         *                          dataArray -> массив содержащий строку которую нужно напечатать
         * Возвращаемое значение :  смотри возвращаемое значение в n3310lcd.h
         */
        public byte LcdStr(byte size, string dataArray)
        {
            byte tmpIdx = 0;
            byte response;
            /*while (dataArray[tmpIdx] != '\0')
            {
                // Выводим символ
                response = LcdChr(size, dataArray[tmpIdx]);
                // Не стоит волноваться если произойдет OUT_OF_BORDER,
                // строка будет печататься дальше из начала дисплея
                if (response == OUT_OF_BORDER)
                    return OUT_OF_BORDER;
                // Увеличиваем указатель
                tmpIdx++;
            }*/
            for (tmpIdx = 0; tmpIdx < dataArray.Length; tmpIdx++)
            {
                response = LcdChr(size, dataArray[tmpIdx]);
                if (response == OUT_OF_BORDER)
                    return OUT_OF_BORDER;
            }
            UpdateLcd = true;
            return OK;
        }



        /*
         * Имя                   :  LcdFStr
         * Описание              :  Эта функция предназначена для печати строки которая хранится в Flash ROM
         * Аргумент(ы)           :  size    -> размер шрифта. Смотри enum в n3310.h
         *                          dataPtr -> указатель на строку которую нужно напечатать
         * Возвращаемое значение :  смотри возвращаемое значение в n3310lcd.h
         * Пример                :  LcdFStr(FONT_1X, PSTR("Hello World"));
         *                          LcdFStr(FONT_1X, &name_of_string_as_array);
         */
        /*byte LcdFStr(LcdFontSize size, const byte* dataPtr )
        {
        byte c;
        byte response;
        for (c = pgm_read_byte(dataPtr ); c; ++dataPtr, c = pgm_read_byte(dataPtr ) )
        {
        // Выводим символ
        response = LcdChr(size, c );
        if(response == OUT_OF_BORDER)
            return OUT_OF_BORDER;
        }   

        return OK;
        }*/



        /*
         * Имя                   :  LcdPixel
         * Описание              :  Отображает пиксель по абсолютным координатам (x,y)
         * Аргумент(ы)           :  x,y  -> абсолютные координаты пикселя
         *                          mode -> Off, On или Xor. Смотри enum в n3310.h
         * Возвращаемое значение :  смотри возвращаемое значение в n3310lcd.h
         */
        public byte LcdPixel(byte x, byte y, byte mode)
        {
            int index;
            byte offset;
            byte data;

            // Защита от выхода за пределы
            if (x >= LCD_X_RES || y >= LCD_Y_RES) return OUT_OF_BORDER;

            // Пересчет индекса и смещения
            index = ((y / LCD_LINES) * LCD_X_RES) + x;
            offset = (byte)(y - ((y / 8) * LCD_LINES));

            data = LcdCache[index];

            // Обработка битов

            // Режим PIXEL_OFF
            if (mode == PIXEL_OFF)
            {
                data &= (byte)(~(0x01 << offset));
            }
            // Режим PIXEL_ON
            else if (mode == PIXEL_ON)
            {
                data |= (byte)(0x01 << offset);
            }
            // Режим PIXEL_XOR
            else if (mode == PIXEL_XOR)
            {
                data ^= (byte)(0x01 << offset);
            }

            // Окончательный результат копируем в кэш
            LcdCache[index] = data;

            if (index < LoWaterMark)
            {
                // Обновляем нижнюю границу
                LoWaterMark = index;
            }

            if (index > HiWaterMark)
            {
                // Обновляем верхнюю границу
                HiWaterMark = index;
            }
            return OK;
        }



        /*
         * Имя                   :  LcdLine
         * Описание              :  Рисует линию между двумя точками на дисплее (алгоритм Брезенхэма)
         * Аргумент(ы)           :  x1, y1  -> абсолютные координаты начала линии
         *                          x2, y2  -> абсолютные координаты конца линии
         *                          mode    -> Off, On или Xor. Смотри enum в n3310.h
         * Возвращаемое значение :  смотри возвращаемое значение в n3310lcd.h
         */
        public byte LcdLine(byte x1, byte y1, byte x2, byte y2, byte mode)
        {
            int dx, dy, stepx, stepy, fraction;
            byte response;

            // dy   y2 - y1
            // -- = -------
            // dx   x2 - x1

            dy = y2 - y1;
            dx = x2 - x1;

            // dy отрицательное
            if (dy < 0)
            {
                dy = -dy;
                stepy = -1;
            }
            else
            {
                stepy = 1;
            }

            // dx отрицательное
            if (dx < 0)
            {
                dx = -dx;
                stepx = -1;
            }
            else
            {
                stepx = 1;
            }

            dx <<= 1;
            dy <<= 1;

            // Рисуем начальную точку
            response = LcdPixel(x1, y1, mode);
            if (response > 0)
                return response;

            // Рисуем следующие точки до конца
            if (dx > dy)
            {
                fraction = dy - (dx >> 1);
                while (x1 != x2)
                {
                    if (fraction >= 0)
                    {
                        y1 += (byte)stepy;
                        fraction -= dx;
                    }
                    x1 += (byte)stepx;
                    fraction += dy;

                    response = LcdPixel(x1, y1, mode);
                    if (response > 0)
                        return response;

                }
            }
            else
            {
                fraction = dx - (dy >> 1);
                while (y1 != y2)
                {
                    if (fraction >= 0)
                    {
                        x1 += (byte)stepx;
                        fraction -= dy;
                    }
                    y1 += (byte)stepy;
                    fraction += dx;

                    response = LcdPixel(x1, y1, mode);
                    if (response > 0)
                        return response;
                }
            }

            // Установка флага изменений кэша
            UpdateLcd = true;
            return OK;
        }



        /*
         * Имя                   :  LcdCircle
         * Описание              :  Рисует окружность (алгоритм Брезенхэма)
         * Аргумент(ы)           :  x, y   -> абсолютные координаты центра
         *                          radius -> радиус окружности
         *                          mode   -> Off, On или Xor. Смотри enum в n3310.h
         * Возвращаемое значение :  смотри возвращаемое значение в n3310lcd.h
         */
        public byte LcdCircle(byte x, byte y, byte radius, byte mode)
        {
            sbyte xc = 0;
            sbyte yc = 0;
            sbyte p = 0;

            if (x >= LCD_X_RES || y >= LCD_Y_RES) return OUT_OF_BORDER;

            yc = (sbyte)radius;
            p = (sbyte)(3 - (radius << 1));
            while (xc <= yc)
            {
                LcdPixel((byte)(x + xc), (byte)(y + yc), mode);
                LcdPixel((byte)(x + xc), (byte)(y - yc), mode);
                LcdPixel((byte)(x - xc), (byte)(y + yc), mode);
                LcdPixel((byte)(x - xc), (byte)(y - yc), mode);
                LcdPixel((byte)(x + yc), (byte)(y + xc), mode);
                LcdPixel((byte)(x + yc), (byte)(y - xc), mode);
                LcdPixel((byte)(x - yc), (byte)(y + xc), mode);
                LcdPixel((byte)(x - yc), (byte)(y - xc), mode);
                if (p < 0) p += (sbyte)((xc++ << 2) + 6);
                else p += (sbyte)(((xc++ - yc--) << 2) + 10);
            }

            // Установка флага изменений кэша
            UpdateLcd = true;
            return OK;
        }


        /*
         * Имя                   :  LcdSingleBar
         * Описание              :  Рисует один закрашенный прямоугольник
         * Аргумент(ы)           :  baseX  -> абсолютная координата x (нижний левый угол)
         *                          baseY  -> абсолютная координата y (нижний левый угол)
         *                          height -> высота (в пикселях)
         *                          width  -> ширина (в пикселях)
         *                          mode   -> Off, On или Xor. Смотри enum в n3310.h
         * Возвращаемое значение :  смотри возвращаемое значение в n3310lcd.h
         */
        public byte LcdSingleBar(byte baseX, byte baseY, byte height, byte width, byte mode)
        {
            byte tmpIdxX, tmpIdxY, tmp;

            byte response;

            // Проверка границ
            if ((baseX >= LCD_X_RES) || (baseY >= LCD_Y_RES)) return OUT_OF_BORDER;

            if (height > baseY)
                tmp = 0;
            else
                tmp = (byte)(baseY - height + 1);

            // Рисование линий
            for (tmpIdxY = tmp; tmpIdxY <= baseY; tmpIdxY++)
            {
                for (tmpIdxX = baseX; tmpIdxX < (baseX + width); tmpIdxX++)
                {
                    response = LcdPixel(tmpIdxX, tmpIdxY, mode);
                    if (response > 0)
                        return response;

                }
            }

            // Установка флага изменений кэша
            UpdateLcd = true;
            return OK;
        }



        /*
         * Имя                   :  LcdBars
         * Описание              :  Рисует группу закрашенных прямоугольников (в режиме PIXEL_ON)
         * Аргумент(ы)           :  data[]     -> данные которые нужно отобразить
         *                          numbBars   -> количество прямоугольников
         *                          width      -> ширина (в пикселях)
         *                          multiplier -> множитель для высоты
         * Возвращаемое значение :  смотри возвращаемое значение в n3310lcd.h
         * Примечание            :  Пожалуйста проверьте значения EMPTY_SPACE_BARS, BAR_X, BAR_Y в n3310.h
         * Пример                :  byte example[5] = {1, 2, 3, 4, 5};
         *                          LcdBars(example, 5, 3, 2);
         */
        public byte LcdBars(byte[] data, byte numbBars, byte width, byte multiplier)
        {
            byte b;
            byte tmpIdx = 0;
            byte response;

            for (b = 0; b < numbBars; b++)
            {
                // Защита от выхода за пределы
                if (tmpIdx > LCD_X_RES - 1) return OUT_OF_BORDER;

                // Расчет значения x
                tmpIdx = (byte)(((width + EMPTY_SPACE_BARS) * b) + BAR_X);

                // Рисуем один прямоугольник
                response = LcdSingleBar(tmpIdx, BAR_Y, (byte)(data[b] * multiplier), width, PIXEL_ON);
                if (response == OUT_OF_BORDER)
                    return response;
            }

            // Установка флага изменений кэша
            UpdateLcd = true;
            return OK;

        }



        /*
         * Имя                   :  LcdRect
         * Описание              :  Рисует незакрашенный прямоугольник
         * Аргумент(ы)           :  x1    -> абсолютная координата x левого верхнего угла
         *                          y1    -> абсолютная координата y левого верхнего угла
         *                          x2    -> абсолютная координата x правого нижнего угла
         *                          y2    -> абсолютная координата y правого нижнего угла
         *                          mode  -> Off, On или Xor. Смотри enum в n3310.h
         * Возвращаемое значение :  смотри возвращаемое значение в n3310lcd.h
         */
        public byte LcdRect(byte x1, byte y1, byte x2, byte y2, byte mode)
        {
            byte tmpIdx;

            // Проверка границ
            if ((x1 >= LCD_X_RES) || (x2 >= LCD_X_RES) || (y1 >= LCD_Y_RES) || (y2 >= LCD_Y_RES))
                return OUT_OF_BORDER;

            if ((x2 > x1) && (y2 > y1))
            {
                // Рисуем горизонтальные линии
                for (tmpIdx = x1; tmpIdx <= x2; tmpIdx++)
                {
                    LcdPixel(tmpIdx, y1, mode);
                    LcdPixel(tmpIdx, y2, mode);
                }

                // Рисуем вертикальные линии
                for (tmpIdx = y1; tmpIdx <= y2; tmpIdx++)
                {
                    LcdPixel(x1, tmpIdx, mode);
                    LcdPixel(x2, tmpIdx, mode);
                }

                // Установка флага изменений кэша
                UpdateLcd = true;
            }
            return OK;
        }



        /*
         * Имя                   :  LcdImage
         * Описание              :  Рисует картинку из массива сохраненного в Flash ROM
         * Аргумент(ы)           :  Указатель на массив картинки
         * Возвращаемое значение :  Нет
         */
        public void LcdImage(byte[] imageData)
        {
            // Инициализация указателя кэша
            LcdCacheIdx = 0;
            // В пределах кэша
            int len = imageData.Length;
            if (len < LCD_CACHE_SIZE)
            {
                for (LcdCacheIdx = 0; LcdCacheIdx < len; LcdCacheIdx++)
                {
                    // Копируем данные из массива в кэш
                    LcdCache[LcdCacheIdx] = imageData[LcdCacheIdx];
                }
                for (; LcdCacheIdx < LCD_CACHE_SIZE; LcdCacheIdx++)
                {
                    LcdCache[LcdCacheIdx] = 0x00;
                }
            }
            else
                for (LcdCacheIdx = 0; LcdCacheIdx < LCD_CACHE_SIZE; LcdCacheIdx++)
                {
                    // Копируем данные из массива в кэш
                    LcdCache[LcdCacheIdx] = imageData[LcdCacheIdx];
                }

            // Оптимизация от Jakub Lasinski (March 14 2009)
            //memcpy_P(LcdCache, imageData, LCD_CACHE_SIZE);  // Тоже самое что и выше, но занимает меньше памяти и быстрее выполняется

            // Сброс указателей границ в максимальное значение
            LoWaterMark = 0;
            HiWaterMark = LCD_CACHE_SIZE - 1;

            // Установка флага изменений кэша
            UpdateLcd = true;
        }


        #endregion
    }
}
