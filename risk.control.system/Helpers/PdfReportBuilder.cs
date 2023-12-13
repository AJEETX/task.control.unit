using System.Globalization;

using Gehtsoft.PDFFlow.Builder;
using Gehtsoft.PDFFlow.Models.Enumerations;
using Gehtsoft.PDFFlow.Models.Shared;
using Gehtsoft.PDFFlow.UserUtils;
using Gehtsoft.PDFFlow.Utils;

using risk.control.system.Models.ViewModel;
using risk.control.system.AppConstant;

namespace risk.control.system.Helpers
{
    public class PdfReportBuilder
    {
        public BoardingData BoardingData { get; internal set; }
        public TicketData1 TicketData1 { get; internal set; }
        public BoardingData BoardingData0 { get; internal set; }
        public TicketData1 TicketData0 { get; internal set; }
        public List<string> WhatsNextData { get; internal set; }
        public TicketData TicketData { get; internal set; }
        public ConcertData ConcertData { get; internal set; }

        internal static readonly CultureInfo DocumentLocale
            = new CultureInfo("en-US");

        internal const PageOrientation Orientation
            = PageOrientation.Portrait;

        internal static readonly Box Margins = new Box(29, 20, 29, 20);

        internal static readonly XUnit PageWidth =
            (PredefinedSizeBuilder.ToSize(PaperSize.Letter).Width -
                (Margins.Left + Margins.Right));

        internal static readonly FontBuilder FNT9 = Fonts.Helvetica(9f);
        internal static readonly FontBuilder FNT10 = Fonts.Helvetica(10f);
        internal static readonly FontBuilder FNT12 = Fonts.Helvetica(12f);
        internal static readonly FontBuilder FNT12B = Fonts.Helvetica(12f).SetBold(true);
        internal static readonly FontBuilder FNT20 = Fonts.Helvetica(20f);
        internal static readonly FontBuilder FNT19B = Fonts.Helvetica(19f).SetBold();

        internal static readonly FontBuilder FNT8 = Fonts.Helvetica(8f);

        internal static readonly FontBuilder FNT8_G =
            Fonts.Helvetica(8f).SetColor(Color.Gray);

        internal static readonly FontBuilder FNT9B =
            Fonts.Helvetica(9f).SetBold();

        internal static readonly FontBuilder FNT11B =
            Fonts.Helvetica(11f).SetBold();

        internal static readonly FontBuilder FNT15 = Fonts.Helvetica(15f);
        internal static readonly FontBuilder FNT16 = Fonts.Helvetica(16f);

        internal static readonly FontBuilder FNT16_R =
            Fonts.Helvetica(16f).SetColor(Color.Red);

        internal static readonly FontBuilder FNT17 = Fonts.Helvetica(17f);
        internal static readonly FontBuilder FNT18 = Fonts.Helvetica(18f);

        internal static readonly BoardingCell EMPTY_ITEM = new BoardingCell("", new FontText[0]);
        private string imgPath = string.Empty;

        internal DocumentBuilder Build(string imagePath)
        {
            imgPath = imagePath;
            DocumentBuilder documentBuilder = DocumentBuilder.New();
            var concertSection = documentBuilder.AddSection();
            BoardingCell[,] boardingItems = GetBoardingItems();
            BoardingCell[,] boardingItems0 = GetBoardingItems0();

            concertSection
                 .SetOrientation(Orientation)
                 .SetMargins(Margins);

            addConcertTable(concertSection);
            FillBoardingHandBugTable(concertSection.AddTable(), boardingItems, boardingItems0);
            addInfoTable(concertSection);
            addCounterFoil(concertSection);

            return documentBuilder;
        }

        public void addConcertTable(SectionBuilder section)
        {
            var concertTable = section.AddTable()
                .SetContentRowStyleBorder(borderBuilder =>
                    borderBuilder.SetStroke(Stroke.None));

            concertTable
                .SetWidth(XUnit.FromPercent(100))
                .AddColumnPercentToTable("", 20)
                .AddColumnPercentToTable("", 30)
                .AddColumnPercentToTable("", 20)
                .AddColumnPercentToTable("", 30);

            var row1Builder = concertTable.AddRow();
            AddLogoImage(row1Builder.AddCell("", 0, 2));
            AddConcertData(row1Builder.AddCell("", 3, 0)
                .SetPadding(32, 0, 0, 0));

            var row2Builder = concertTable.AddRow();
            row2Builder.AddCell();
            No(row2Builder.AddCell("").SetFont(FNT10)
                .SetPadding(32, 0, 0, 0));
            FillTicketData(row2Builder.AddCell());
            FillPersonalInfo(row2Builder.AddCell());
        }

        public void addInfoTable(SectionBuilder section)
        {
            var infoTable = section.AddTable()
                .SetContentRowStyleBorder(borderBuilder =>
                    borderBuilder.SetStroke(Stroke.None));

            infoTable
                .SetMarginTop(9f)
                .SetWidth(XUnit.FromPercent(100))
                .AddColumnPercentToTable("", 50)
                .AddColumnPercentToTable("", 25)
                .AddColumnPercentToTable("", 25);

            var row3Builder = infoTable.AddRow();
            FillRuleA(start: 0, end: 10, row3Builder.AddCell("").SetFont(FNT10));
            FillRuleP(row3Builder.AddCell("", 2, 0).SetFont(FNT10));

            var row4Builder = infoTable.AddRow();
            FillBandlist(row4Builder.AddCell("").SetFont(FNT12));
            row4Builder.AddCell("")
                .AddImage(Path.Combine(imgPath, "images", "CT_Location.png")).SetHeight(400)
                .SetMarginTop(9);
            AddContactInfo(row4Builder.AddCell("").SetFont(FNT12));
        }

        private void addCounterFoil(SectionBuilder section)
        {
            var counterFoil = section.AddTable()
                .SetContentRowStyleBorder(borderBuilder =>
                    borderBuilder.SetStroke(Stroke.None));

            counterFoil
                .SetMarginTop(10f)
                .SetWidth(XUnit.FromPercent(100))
                .AddColumnPercentToTable("", 50)
                .AddColumnPercentToTable("", 16)
                .AddColumnPercentToTable("", 20)
                .AddColumnPercentToTable("", 14);

            var row5Builder = counterFoil.AddRow();
            YourTicket(row5Builder.AddCell("")
                .SetPadding(0, 2, 0, 0));
            AddConcertData(row5Builder.AddCell("", 3, 0));

            var row6Builder = counterFoil.AddRow();
            row6Builder.AddCell()
                .AddImage(Path.Combine(imgPath, "images", "hdfc.jpg")).SetHeight(100);
            FillTicketDataCounterFoil(row6Builder.AddCell());
            FillPersonalInfoCounterFoil(row6Builder.AddCell());
            row6Builder.AddCell()
                .AddQRCodeUrl("https://icheckify.co.in/", 4, Color.Black, Color.White, false).SetWidth(153);

            var row7Builder = counterFoil.AddRow();
            row7Builder.AddCell();
            row7Builder.AddCell();
            row7Builder.AddCell();
            row7Builder.AddCell(TicketData.Eticket).SetFont(FNT10);
        }

        private void AddConcertData(TableCellBuilder cellBuilder)
        {
            cellBuilder
                .AddParagraph("Investigation Report").SetFont(FNT19B);
            cellBuilder
                .AddParagraph("07/11/2023  9:00PM").SetFont(FNT12)
                .SetBorderStroke(strokeLeft: Stroke.None, strokeTop: Stroke.None, strokeRight: Stroke.None, strokeBottom: Stroke.Solid)
                .SetBorderWidth(2);
        }

        private void AddLogoImage(TableCellBuilder cellBuilder)
        {
            cellBuilder
                .SetPadding(2, 2, 2, 0);
            cellBuilder
                .AddImage(Path.Combine(imgPath, "images", "verify.png")).SetHeight(340);
        }

        private void No(TableCellBuilder cellBuilder)
        {
            cellBuilder
               .AddParagraph("Agency Name");
            cellBuilder
                .AddParagraph(TicketData.Eticket).SetLineSpacing(1.5f);
            cellBuilder
                .AddQRCodeUrl("https://icheckify.co.in/", 4,
                              Color.Black, Color.White, false).SetHeight(100);
        }

        private void FillRuleA(int start, int end, TableCellBuilder cellBuilder)
        {
            cellBuilder.AddParagraph(ConcertData.TitleRulesOfAttendance).SetFont(FNT12B).SetMargins(10, 10, 1, 4);
            cellBuilder.SetBorderStroke(strokeLeft: Stroke.Solid, strokeTop: Stroke.Solid, strokeRight: Stroke.None, strokeBottom: Stroke.Solid);

            foreach (var item in ConcertData.RulesOfAttendance)
            {
                cellBuilder.AddParagraph(item).SetFont(FNT9).SetMargins(20, 0, 10, 2);
            }
        }

        private void FillRuleP(TableCellBuilder cellBuilder)
        {
            cellBuilder.AddParagraph(ConcertData.TitleRulesOfPurchase).SetFont(FNT12B).SetMargins(10, 10, 1, 4);
            cellBuilder.SetBorderStroke(strokeLeft: Stroke.None, strokeTop: Stroke.Solid,
                    strokeRight: Stroke.Solid, strokeBottom: Stroke.Solid);
            cellBuilder.AddParagraph(ConcertData.RulesOfPurchase).SetFont(FNT9).SetLineSpacing(1.2f).SetMargins(10, 0, 10, 4);
        }

        private void FillBandlist(TableCellBuilder cellBuilder)
        {
            cellBuilder.SetBorderStroke(Stroke.None);
            cellBuilder.AddParagraph(ConcertData.TitleBandsList).SetFont(FNT12B).SetMargins(0, 20, 1, 4);
            cellBuilder.AddParagraph(ConcertData.BandsList).SetFont(FNT9).SetLineSpacing(1.2f).SetMargins(0, 0, 30, 4);
            cellBuilder.AddParagraph("");
        }

        private void AddContactInfo(TableCellBuilder cellBuilder)
        {
            cellBuilder.SetBorderStroke(Stroke.None).SetPadding(11, 11, 0, 0);
            cellBuilder.AddParagraph(ConcertData.TitleHowtoFind).SetFont(FNT12B).SetMargins(0, 9, 1, 4);
            cellBuilder.AddParagraph(ConcertData.HowToFind).SetFont(FNT9);
            cellBuilder.AddParagraph(ConcertData.TitleLearn).SetFont(FNT12B).SetMarginTop(10);
            cellBuilder.AddParagraph("").SetFont(FNT9)
                .SetAlignment(HorizontalAlignment.Left)
                .AddUrl(ConcertData.Facebook);
            cellBuilder.AddParagraph("").SetFont(FNT9)
                .SetAlignment(HorizontalAlignment.Left)
                .AddUrl(ConcertData.Twitter);
            cellBuilder.AddParagraph(ConcertData.Instagram).SetFont(FNT9)
                .SetAlignment(HorizontalAlignment.Left);
        }

        private void FillTicketData(TableCellBuilder cellBuilder)
        {
            cellBuilder.AddParagraph("Policy #").SetLineSpacing(1.4f);
            cellBuilder.AddParagraph("Claim Type").SetLineSpacing(1.4f);
            cellBuilder.AddParagraph("Insured Amount").SetLineSpacing(1.4f);
            cellBuilder.AddParagraph("Customer Name").SetLineSpacing(1.4f);
            cellBuilder.AddParagraph("Reason").SetLineSpacing(1.4f);
            cellBuilder.AddParagraph("Verified Address").SetLineSpacing(1.4f);
        }

        private void FillPersonalInfo(TableCellBuilder cellBuilder)
        {
            cellBuilder.AddParagraph(TicketData.Admission).SetLineSpacing(1.4f);
            cellBuilder.AddParagraph(TicketData.TicketType).SetLineSpacing(1.4f);
            cellBuilder.AddParagraph(TicketData.Price).SetLineSpacing(1.4f);
            cellBuilder.AddParagraph(TicketData.Name).SetLineSpacing(1.4f);
            cellBuilder.AddParagraph(TicketData.Venue).SetLineSpacing(1.4f);
            cellBuilder.AddParagraph(TicketData.Address).SetLineSpacing(1.4f);
        }

        private void YourTicket(TableCellBuilder cellBuilder)
        {
            cellBuilder.AddParagraph(ConcertData.CounterFoil).SetFont(FNT9).SetMarginRight(30);
        }

        private void FillTicketDataCounterFoil(TableCellBuilder cellBuilder)
        {
            cellBuilder.SetBorderStroke(Stroke.None);
            cellBuilder.AddParagraph("Policy #").SetLineSpacing(1.4f);
            cellBuilder.AddParagraph("Claim type").SetLineSpacing(1.4f);
            cellBuilder.AddParagraph("Insured Amount").SetLineSpacing(1.4f);
            cellBuilder.AddParagraph("Customer Name").SetLineSpacing(1.4f);
        }

        private void FillPersonalInfoCounterFoil(TableCellBuilder cellBuilder)
        {
            cellBuilder.SetBorderStroke(Stroke.None).SetBold(true);
            cellBuilder.AddParagraph(TicketData.Admission).SetLineSpacing(1.4f);
            cellBuilder.AddParagraph(TicketData.TicketType).SetLineSpacing(1.4f);
            cellBuilder.AddParagraph(TicketData.Price).SetLineSpacing(1.4f);
            cellBuilder.AddParagraph(TicketData.Name).SetLineSpacing(1.4f);
        }

        private void FillBoardingHandBugTable(TableBuilder tableBuilder, BoardingCell[,] boardingItems, BoardingCell[,] boardingItems0)
        {
            tableBuilder
                .SetWidth(XUnit.FromPercent(100))
                .SetBorder(Stroke.None)
                .AddColumnToTable("", 415.5f)
                .AddColumn("", 138.5f);
            FillBoardingTableFirstRow(tableBuilder, boardingItems[0, 0]);
            var rowBuilder = tableBuilder.AddRow();
            rowBuilder.AddCell().AddTable(builder =>
            {
                builder.SetWidth(415.5f);
                FillBoardingTable(builder, boardingItems, 1);
            });
            rowBuilder.AddCell(FillHandBugTableCell);

            var rr = tableBuilder.AddRow();
            rr.AddCell().AddTable(b =>
            {
                b.SetWidth(415.5f);
                b.SetBorder(Stroke.Dashed)
                .AddColumnPercentToTable("", 25)
                .AddColumnPercentToTable("", 25)
                .AddColumnPercentToTable("", 25)
                .AddColumnPercent("", 25);
            });
            rr.AddCell(FillHandBugTableCell0);

            FillBoardingTableFirstRow(tableBuilder, boardingItems0[0, 0]);
            var rb = tableBuilder.AddRow();
            rb.AddCell().AddTable(builder =>
            {
                builder.SetWidth(415.5f);
                FillBoardingTable(builder, boardingItems0, 1);
            });
            rb.AddCell(FillHandBugTableCell1);
        }

        private void FillHandBugTableCell0(TableCellBuilder cellBuilder)
        {
            cellBuilder
                .SetPadding(19, 6, 0, 0)
                .AddParagraph("-")
                .SetFont(FNT9)
                .SetMarginBottom(19);
        }

        private void FillHandBugTableCell(TableCellBuilder cellBuilder)
        {
            cellBuilder
                .SetPadding(19, 6, 0, 0)
                .AddParagraph("Address visited")
                .SetFont(FNT9)
                .SetMarginBottom(19);
            cellBuilder.AddImage(
                Path.Combine(imgPath, "images", "CT_Location.png"),
                XSize.FromHeight(108));
            cellBuilder.AddParagraph("")
               .AddUrl($"https://maps.googleapis.com/maps/api/staticmap?center=32.661839,-97.263680&zoom=14&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C32.661839,-97.263680&key={Applicationsettings.GMAPData}", "map");
        }

        private void FillHandBugTableCell1(TableCellBuilder cellBuilder)
        {
            cellBuilder
                .SetPadding(19, 6, 0, 0)
                .AddParagraph("Location visited")
                .SetFont(FNT9)
                .SetMarginBottom(19);
            cellBuilder.AddImage(
                Path.Combine(imgPath, "images", "CT_Scheme.png"),
                XSize.FromHeight(108));
            cellBuilder.AddParagraph("")
                .AddUrl($"https://maps.googleapis.com/maps/api/staticmap?center=32.661839,-97.263680&zoom=14&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C32.661839,-97.263680&key={Applicationsettings.GMAPData}", "map");
        }

        private void FillBoardingTable(TableBuilder tableBuilder,
         BoardingCell[,] boardingItems, int startRow = 0)
        {
            tableBuilder
                .SetBorder(Stroke.None)
                .AddColumnPercentToTable("", 25)
                .AddColumnPercentToTable("", 25)
                .AddColumnPercentToTable("", 25)
                .AddColumnPercent("", 25);
            int rows = boardingItems.GetLength(0);
            int columns = boardingItems.GetLength(1);
            for (int i = startRow; i < rows; i++)
            {
                for (int k = 0; k < 2; k++)
                {
                    var rowBuilder = tableBuilder.AddRow();
                    if (k == 0)
                    {
                        rowBuilder.ApplyStyle(
                            StyleBuilder.New()
                                .SetPaddingTop(4)
                            );
                    }
                    else if (i < rows - 1)
                    {
                        rowBuilder.ApplyStyle(
                            StyleBuilder.New()
                                .SetBorderBottom(0.5f,
                                    Stroke.Solid, Color.Black)
                                .SetPaddingBottom(4)
                            );
                    }
                    for (int j = 0; j < columns; j++)
                    {
                        BoardingCell bi = boardingItems[i, j];
                        if (!bi.isEmpty())
                        {
                            var cellBuilder = rowBuilder.AddCell();
                            if (bi.colSpan > 1)
                            {
                                cellBuilder.SetColSpan(bi.colSpan);
                            }
                            if (k == 0)
                            {
                                cellBuilder
                                    .AddParagraph(bi.name).SetFont(FNT9);
                            }
                            else
                            {
                                if (bi.image != null)
                                {
                                    cellBuilder.AddTable(builder =>
                                    {
                                        ImageThenText(builder, bi);
                                    });
                                }
                                else
                                {
                                    TextOnly(cellBuilder.AddParagraph(), bi);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ImageThenText(TableBuilder tableBuilder, BoardingCell bi)
        {
            tableBuilder
                .SetWidth(XUnit.FromPercent(100))
                .SetBorder(Stroke.None)
                .AddColumnToTable("", 13)
                .AddColumn("");
            var rowBuilder = tableBuilder.AddRow();
            rowBuilder.AddCell()
                .SetPadding(0, 4, 0, 0)
                //.SetVerticalAlignment(VerticalAlignment.Bottom)
                .AddImage(Path.Combine(imgPath, "images", bi.image),
                    XSize.FromWidth(11));
            TextOnly(rowBuilder.AddCell().AddParagraph(), bi);
        }

        private void TextOnly(ParagraphBuilder paragraphBuilder, BoardingCell bi)
        {
            foreach (FontText ft in bi.fontTexts)
            {
                paragraphBuilder.AddText(ft.text).SetFont(ft.font);
            }
        }

        private void FillBoardingTableFirstRow(TableBuilder tableBuilder,
                BoardingCell bi)
        {
            for (int k = 0; k < 2; k++)
            {
                var rowBuilder = tableBuilder.AddRow();
                if (k == 1)
                {
                    rowBuilder.ApplyStyle(
                        StyleBuilder.New()
                            .SetBorderBottom(0.5f, Stroke.Solid, Color.Black)
                            .SetPaddingBottom(6)
                        );
                }
                var cellBuilder = rowBuilder.AddCell();
                cellBuilder.SetColSpan(2);
                if (k == 0)
                {
                    cellBuilder.SetFont(FNT9).AddParagraph(bi.name);
                }
                else
                {
                    if (bi.image != null)
                    {
                        cellBuilder.AddTable(builder =>
                        {
                            ImageThenText(builder, bi);
                        });
                    }
                    else
                    {
                        TextOnly(cellBuilder.AddParagraph(), bi);
                    }
                }
            }
        }

        private BoardingCell[,] GetBoardingItems()
        {
            BoardingCell[,] result =
            {
                {
                new BoardingCell("Investigation Type", FNT18,
                    TicketData1.Passenger, 4),
                    EMPTY_ITEM,
                    EMPTY_ITEM,
                    EMPTY_ITEM
                },
                {
                new BoardingCell("Person Name", new FontText[] {
                    new FontText (FNT12, BoardingData.DepartureAirport + " / "),
                    new FontText (FNT12B, BoardingData.DepartureAbvr)
                }, 2),
                EMPTY_ITEM,
                new BoardingCell("Address", new FontText[] {
                    new FontText (FNT12, BoardingData.ArrivalAirport + " / "),
                    new FontText (FNT12B, BoardingData.ArrivalAbvr)
                }, 2),
                EMPTY_ITEM
                },
                {
                new BoardingCell("Match", FNT16_R, BoardingData.Flight),
                new BoardingCell("Contact Number", FNT16, BoardingData.BoardingGate),
                new BoardingCell("Status", FNT16, BoardingData.Class + " "
                    +  BoardingData.ClassAdd),
                new BoardingCell("Verified", FNT16_R, BoardingData.Seat,
                    Path.Combine(imgPath, "images", "BP_seat_2x.png"))
                },
                {
                new BoardingCell("Visit Date", FNT16,
                    BoardingData.DepartureTime.ToString(
                                "dd MMMM", DocumentLocale)),
                new BoardingCell("Time", FNT16,
                    BoardingData.BoardingTill.ToString(
                                "HH:mm", DocumentLocale)),
                new BoardingCell("", FNT16,
                    ""),
                new BoardingCell("Weather", FNT16,
                    BoardingData.Arrival.ToString(
                                "HH:mm", DocumentLocale))
                }
            };
            return result;
        }

        private BoardingCell[,] GetBoardingItems0()
        {
            BoardingCell[,] result =
            {
                {
                new BoardingCell("Investigation Type", FNT18,
                    TicketData0.Passenger, 4),
                    EMPTY_ITEM,
                    EMPTY_ITEM,
                    EMPTY_ITEM
                },
                {
                new BoardingCell("Document Name", new FontText[] {
                    new FontText (FNT12, BoardingData0.DepartureAirport + " / "),
                    new FontText (FNT12B, BoardingData0.DepartureAbvr)
                }, 2),
                EMPTY_ITEM,
                new BoardingCell("Address", new FontText[] {
                    new FontText (FNT12, BoardingData0.ArrivalAirport + " / "),
                    new FontText (FNT12B, BoardingData0.ArrivalAbvr)
                }, 2),
                EMPTY_ITEM
                },
                {
                new BoardingCell("Match", FNT16_R, BoardingData0.Flight),
                new BoardingCell("PAN Number", FNT16, BoardingData0.BoardingGate),
                new BoardingCell("Status", FNT16, BoardingData0.Class + " "
                    +  BoardingData0.ClassAdd),
                new BoardingCell("Verified", FNT16_R, BoardingData0.Seat,
                    Path.Combine(imgPath, "images", "PAN.jpg"))
                },
                {
                new BoardingCell("Visit Date", FNT16,
                    BoardingData0.DepartureTime.ToString(
                                "dd MMMM", DocumentLocale)),
                new BoardingCell("Time", FNT16,
                    BoardingData0.BoardingTill.ToString(
                                "HH:mm", DocumentLocale)),
                new BoardingCell("", FNT16,
                    ""),
                new BoardingCell("Weather", FNT16,
                    BoardingData0.Arrival.ToString(
                                "HH:mm", DocumentLocale))
                }
            };
            return result;
        }

        internal struct BoardingCell
        {
            internal string name;
            internal FontText[] fontTexts;
            internal string image;
            internal int colSpan;

            internal BoardingCell(string name, FontBuilder font, string text, int colSpan = 1) : this(name, font, text, null, colSpan)
            {
            }

            internal BoardingCell(string name, FontBuilder font, string text, string image, int colSpan = 1)
            {
                this.name = name;
                fontTexts = new FontText[] { new FontText(font, text) };
                this.image = image;
                this.colSpan = colSpan;
            }

            internal BoardingCell(string name, FontText[] fontTexts, int colSpan = 1)
            {
                this.name = name;
                this.fontTexts = fontTexts;
                this.image = null;
                this.colSpan = colSpan;
            }

            internal bool isEmpty()
            {
                return fontTexts.Length == 0;
            }
        }

        internal struct FontText
        {
            internal FontBuilder font;
            internal string text;

            internal FontText(FontBuilder font, string text)
            {
                this.font = font;
                this.text = text;
            }
        }
    }
}