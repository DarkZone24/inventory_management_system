using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using MySqlConnector;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace InventorySystemCsharp
{
    public partial class Manager_home : Form
    {
        private const string CONNECTION_STRING = @"datasource=192.168.254.177;port=3308;SslMode=none;username=root;password=Khw67(js90qYjsp7;database=inv_management;";

        public Manager_home()
        {
            InitializeComponent();
            InitializeSlidePanel();
        }

        private void InitializeSlidePanel()
        {
            slide_panel.Height = add.Height;
            slide_panel.Top = add.Top;
            additem_panel.BringToFront();
        }

        private void Manager_home_Load(object sender, EventArgs e)
        {
            InitializeAllPanels();
        }

        private void InitializeAllPanels()
        {
            // Initialize all auto-number fields and fill grids
            SetAutoNumberField(itemcode);
            SetAutoNumberField(u_itemcodeTxt);
            SetAutoNumberField(d_itemcodeTxt);
            SetAutoNumberField(p_order_idTxt);
            SetAutoNumberField(unp_orderidTxt);

            RefreshAllGridViews();
        }

        private void SetAutoNumberField(TextBox textBox)
        {
            textBox.Enabled = false;
            textBox.Text = "Id Auto Number";
        }

        private void RefreshAllGridViews()
        {
            FillGridView(itemlist, "SELECT * FROM spareparts1");
            FillGridView(u_dataGridView, "SELECT * FROM spareparts1");
            FillGridView(d_item_dataGridView, "SELECT * FROM spareparts1");
            FillGridView(paid_dataGridView1, "SELECT * FROM orders WHERE paid = 'yes'");
            FillGridView(unp_dataGridView, "SELECT * FROM orders WHERE paid = 'no'");
        }

        // Generic method to fill any DataGridView
        private void FillGridView(DataGridView grid, string query)
        {
            using (var conn = new MySqlConnection(CONNECTION_STRING))
            {
                var adapter = new MySqlDataAdapter(query, conn);
                var table = new DataTable();
                adapter.Fill(table);
                grid.DataSource = table;
            }
        }

        // Generic method for database operations
        private bool ExecuteQuery(string query, string successMessage)
        {
            try
            {
                using (var conn = new MySqlConnection(CONNECTION_STRING))
                {
                    var cmd = new MySqlCommand(query, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show(successMessage);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return false;
            }
        }

        // Navigation methods
        private void NavigateToPanel(Button button, Panel panel, Action clearAction = null)
        {
            slide_panel.Height = button.Height;
            slide_panel.Top = button.Top;
            panel.BringToFront();
            clearAction?.Invoke();
        }

        #region Navigation Events
        private void close_btn_Click(object sender, EventArgs e) => Close();

        private void logout_btn_Click(object sender, EventArgs e)
        {
            new Login().Show();
            Close();
        }

        private void add_Click(object sender, EventArgs e) =>
            NavigateToPanel(add, additem_panel, ClearAddItemFields);

        private void update_Click(object sender, EventArgs e) =>
            NavigateToPanel(update, updateitems_panel, ClearUpdateFields);

        private void delete_Click(object sender, EventArgs e) =>
            NavigateToPanel(delete, deleteitem_panel, ClearDeleteFields);

        private void paid_orders_Click(object sender, EventArgs e) =>
            NavigateToPanel(paid_orders, paid_orders_panel, ClearPaidOrderFields);

        private void unpaid_orders_Click(object sender, EventArgs e) =>
            NavigateToPanel(unpaid_orders, unp_order_panel, ClearUnpaidOrderFields);
        #endregion

        #region Clear Field Methods
        private void ClearAddItemFields()
        {
            model.Clear();
            part.Clear();
            price.Clear();
            instock.Clear();
            comboBox1.SelectedIndex = -1;
            SetAutoNumberField(itemcode);
            FillGridView(itemlist, "SELECT * FROM spareparts1");
        }

        private void ClearUpdateFields()
        {
            u_modelTxt.Clear();
            u_partTxt.Clear();
            u_priceTxt.Clear();
            u_stockTxt.Clear();
            u_typeCombo.SelectedIndex = -1;
            SetAutoNumberField(u_itemcodeTxt);
            FillGridView(u_dataGridView, "SELECT * FROM spareparts1");
        }

        private void ClearDeleteFields()
        {
            d_modelTxt.Clear();
            d_partTxt.Clear();
            d_priceTxt.Clear();
            d_instockTxt.Clear();
            d_typeCombo.SelectedIndex = -1;
            SetAutoNumberField(d_itemcodeTxt);
            FillGridView(d_item_dataGridView, "SELECT * FROM spareparts1");
        }

        private void ClearPaidOrderFields()
        {
            p_order_detailsTxt.Clear();
            p_partTxt.Clear();
            p_order_priceTxt.Clear();
            p_order_paidTxt.Clear();
            SetAutoNumberField(p_order_idTxt);
            FillGridView(paid_dataGridView1, "SELECT * FROM orders WHERE paid = 'yes'");
        }

        private void ClearUnpaidOrderFields()
        {
            unp_orderdetailsTxt.Clear();
            unp_partTxt.Clear();
            unp_priceTxt.Clear();
            unp_ispaidTxt.Clear();
            SetAutoNumberField(unp_orderidTxt);
            FillGridView(unp_dataGridView, "SELECT * FROM orders WHERE paid = 'no'");
        }
        #endregion

        #region CRUD Operations
        private void additem_Click(object sender, EventArgs e)
        {
            if (!ValidateFields(model.Text, part.Text, comboBox1.Text, price.Text, instock.Text))
                return;

            var query = $"INSERT INTO spareparts1 (model, part, type, price, instock) VALUES ('{model.Text.Trim()}', '{part.Text.Trim()}', '{comboBox1.Text.Trim()}', '{price.Text.Trim()}', '{instock.Text.Trim()}')";

            if (ExecuteQuery(query, "Item added successfully!"))
            {
                ClearAddItemFields();
            }
        }

        private void u_itemBtn_Click(object sender, EventArgs e)
        {
            if (!ValidateFields(u_modelTxt.Text, u_partTxt.Text, u_typeCombo.Text, u_priceTxt.Text, u_stockTxt.Text))
                return;

            var query = $"UPDATE spareparts1 SET model='{u_modelTxt.Text}', part='{u_partTxt.Text}', type='{u_typeCombo.Text}', price='{u_priceTxt.Text}', instock='{u_stockTxt.Text}' WHERE id='{u_itemcodeTxt.Text}'";

            if (ExecuteQuery(query, "Item updated successfully!"))
            {
                ClearUpdateFields();
            }
        }

        private void del_item_btn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(d_itemcodeTxt.Text) || d_itemcodeTxt.Text == "Id Auto Number")
            {
                MessageBox.Show("Please select an item to delete!");
                return;
            }

            var query = $"DELETE FROM spareparts1 WHERE id='{d_itemcodeTxt.Text}'";

            if (ExecuteQuery(query, "Item deleted successfully!"))
            {
                ClearDeleteFields();
            }
        }
        #endregion

        #region Order Management
        private void make_unPaid_btn_Click(object sender, EventArgs e) =>
            UpdateOrderStatus(p_order_idTxt.Text, "no", "Order marked as Unpaid!", ClearPaidOrderFields);

        private void cancel_order_btn_Click(object sender, EventArgs e) =>
            UpdateOrderStatus(p_order_idTxt.Text, "cancelled", "Order cancelled!", ClearPaidOrderFields);

        private void unp_make_btn_Click(object sender, EventArgs e) =>
            UpdateOrderStatus(unp_orderidTxt.Text, "yes", "Order marked as Paid!", ClearUnpaidOrderFields);

        private void unp_cancelorder_btn_Click(object sender, EventArgs e) =>
            UpdateOrderStatus(unp_orderidTxt.Text, "cancelled", "Order cancelled!", ClearUnpaidOrderFields);

        private void UpdateOrderStatus(string orderId, string status, string message, Action refreshAction)
        {
            if (string.IsNullOrEmpty(orderId) || orderId == "Id Auto Number")
            {
                MessageBox.Show("Please select an order first!");
                return;
            }

            var query = $"UPDATE orders SET paid='{status}' WHERE id='{orderId}'";

            if (ExecuteQuery(query, message))
            {
                refreshAction();
            }
        }
        #endregion

        #region Grid Events
        private void itemlist_CellContentClick(object sender, DataGridViewCellEventArgs e) =>
            PopulateFields(itemlist, e, itemcode, model, part, comboBox1, price, instock);

        private void u_dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e) =>
            PopulateFields(u_dataGridView, e, u_itemcodeTxt, u_modelTxt, u_partTxt, u_typeCombo, u_priceTxt, u_stockTxt);

        private void d_item_dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e) =>
            PopulateFields(d_item_dataGridView, e, d_itemcodeTxt, d_modelTxt, d_partTxt, d_typeCombo, d_priceTxt, d_instockTxt);

        private void paid_dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) =>
            PopulateFields(paid_dataGridView1, e, p_order_idTxt, p_order_detailsTxt, p_partTxt, p_order_priceTxt, p_order_paidTxt);

        private void unp_dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e) =>
            PopulateFields(unp_dataGridView, e, unp_orderidTxt, unp_orderdetailsTxt, unp_partTxt, unp_priceTxt, unp_ispaidTxt);

        private void PopulateFields(DataGridView grid, DataGridViewCellEventArgs e, params Control[] controls)
        {
            if (e.RowIndex < 0) return;

            var row = grid.Rows[e.RowIndex];
            for (int i = 0; i < controls.Length && i < row.Cells.Count; i++)
            {
                if (controls[i] is TextBox txt)
                    txt.Text = row.Cells[i].Value?.ToString() ?? "";
                else if (controls[i] is ComboBox combo)
                    combo.Text = row.Cells[i].Value?.ToString() ?? "";
            }
        }
        #endregion

        #region PDF Export
        private void button1_Click(object sender, EventArgs e) =>
            ExportGridToPdf(itemlist, "Item List Report");

        private void button2_Click(object sender, EventArgs e) =>
            ExportGridToPdf(unp_dataGridView, "Unpaid Orders Report");

        private void button3_Click(object sender, EventArgs e) =>
            ExportGridToPdf(paid_dataGridView1, "Paid Orders Report");

        private void ExportGridToPdf(DataGridView grid, string filename)
        {
            var saveDialog = new SaveFileDialog
            {
                FileName = filename,
                DefaultExt = ".pdf",
                Filter = "PDF files (*.pdf)|*.pdf"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                var document = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                var table = new PdfPTable(grid.Columns.Count)
                {
                    WidthPercentage = 100,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };

                var font = new iTextSharp.text.Font(BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1250, BaseFont.EMBEDDED), 10);

                // Add headers
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    var cell = new PdfPCell(new Phrase(column.HeaderText, font))
                    {
                        BackgroundColor = new iTextSharp.text.Color(240, 240, 240)
                    };
                    table.AddCell(cell);
                }

                // Add data
                foreach (DataGridViewRow row in grid.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        table.AddCell(new Phrase(cell.Value?.ToString() ?? "", font));
                    }
                }

                using (var stream = new FileStream(saveDialog.FileName, FileMode.Create))
                {
                    PdfWriter.GetInstance(document, stream);
                    document.Open();
                    document.Add(table);
                    document.Close();
                }

                MessageBox.Show("PDF exported successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting PDF: {ex.Message}");
            }
        }
        #endregion

        private bool ValidateFields(params string[] fields)
        {
            foreach (var field in fields)
            {
                if (string.IsNullOrWhiteSpace(field))
                {
                    MessageBox.Show("Please fill all required fields!");
                    return false;
                }
            }
            return true;
        }
    }
}