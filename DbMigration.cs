using System;
using System.IO;
using Npgsql;
using System.Windows.Forms;

namespace ComputerPartsShop
{
    public static class DbMigration
    {
        private static string connectionString = "Host=192.168.0.152;Port=5432;Username=postgres;Password=123;Database=computer_parts_sales;Command Timeout=3";

        public static void ApplyMigrations()
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    // Остатки товаров теперь хранятся напрямую в таблице Products (колонка StockQuantity)
                    // Таблица ProductStock больше не используется

                    // 3. Проверка и обновление схемы OrderDetails (добавление Price)
                    // Ошибка 42703 указывает, что колонки Price нет.
                    try 
                    {
                        string checkPriceCol = "SELECT COUNT(*) FROM information_schema.columns WHERE table_name='orderdetails' AND column_name='price';";
                        using(NpgsqlCommand cmd = new NpgsqlCommand(checkPriceCol, con))
                        {
                             long count = Convert.ToInt64(cmd.ExecuteScalar());
                             if (count == 0)
                             {
                                 using(NpgsqlCommand alter = new NpgsqlCommand("ALTER TABLE OrderDetails ADD COLUMN Price DECIMAL(18,2) DEFAULT 0;", con))
                                 {
                                     alter.ExecuteNonQuery();
                                 }
                             }
                        }
                    }
                    catch { /* Ignore if fails, maybe table doesn't exist yet (though it should) */ }
                }
            }
            catch (Exception ex)
            {
                // Не крашим приложение, если миграция не прошла (например, нет прав), 
                // но выводим инфо для отладки.
                MessageBox.Show("DbMigration Warning: " + ex.Message);
            }
        }
    }
}
