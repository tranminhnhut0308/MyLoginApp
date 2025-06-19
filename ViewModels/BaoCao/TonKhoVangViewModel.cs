using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MyLoginApp.Helpers;
using MyLoginApp.Models;
using MySqlConnector;
using Microsoft.Maui.Controls;
using MyLoginApp.Models.BaoCao;

namespace MyLoginApp.ViewModels.BaoCao
{
    public class TonKhoVangViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<TonKhoVangModel> _danhSachTonKhoVang;

        public ObservableCollection<TonKhoVangModel> DanhSachTonKhoVang
        {
            get => _danhSachTonKhoVang;
            set
            {
                if (_danhSachTonKhoVang != value)
                {
                    _danhSachTonKhoVang = value;
                    OnPropertyChanged();
                    DanhSachHienThi = _danhSachTonKhoVang;
                }
            }
        }

        private ObservableCollection<TonKhoVangModel> _danhSachHienThi;
        public ObservableCollection<TonKhoVangModel> DanhSachHienThi
        {
            get => _danhSachHienThi ?? DanhSachTonKhoVang;
            set
            {
                _danhSachHienThi = value;
                OnPropertyChanged();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _loadingMessage;
        public string LoadingMessage
        {
            get => _loadingMessage;
            set
            {
                if (_loadingMessage != value)
                {
                    _loadingMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public string _tuKhoaTimKiem;
        public string TuKhoaTimKiem
        {
            get => _tuKhoaTimKiem;
            set
            {
                if (_tuKhoaTimKiem != value)
                {
                    _tuKhoaTimKiem = value;
                    OnPropertyChanged();
                    ThucHienTimKiem();
                }
            }
        }

        public Command LoadDanhSachTonKhoVangCommand { get; }
        public Command ThucHienTimKiemCommand { get; }

        public TonKhoVangViewModel()
        {
            _danhSachTonKhoVang = new ObservableCollection<TonKhoVangModel>();
            DanhSachHienThi = _danhSachTonKhoVang;
            LoadDanhSachTonKhoVangCommand = new Command(async () => await LoadDanhSachTonKhoVang());
            ThucHienTimKiemCommand = new Command(ThucHienTimKiem);

            Task.Run(async () => await LoadDanhSachTonKhoVang());
        }

        public async Task LoadDanhSachTonKhoVang()
        {
            IsBusy = true;
            LoadingMessage = "Đang tải dữ liệu tồn kho vàng...";

            try
            {
                var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                string query = @"
                    SELECT
                        nhom_hang.NHOM_TEN,
                        SUM(danh_muc_hang_hoa.CAN_TONG) AS CAN_TONG,
                        SUM(danh_muc_hang_hoa.TL_HOT) AS TL_HOT,
                        SUM(danh_muc_hang_hoa.CAN_TONG) - SUM(danh_muc_hang_hoa.TL_HOT) AS TL_THUC,
                        SUM(danh_muc_hang_hoa.CONG_GOC) AS CONG_GOC,
                        SUM(danh_muc_hang_hoa.GIA_CONG) AS GIA_CONG,
                        nhom_hang.DON_GIA_BAN AS DON_GIA_BAN,
                        SUM(ton_kho.SL_TON) AS SL_TON
                    FROM
                        danh_muc_hang_hoa,
                        ton_kho,
                        nhom_hang,
                        loai_hang
                    WHERE
                        danh_muc_hang_hoa.HANGHOAID = ton_kho.HANGHOAID
                        AND danh_muc_hang_hoa.NHOMHANGID = nhom_hang.NHOMHANGID
                        AND danh_muc_hang_hoa.LOAIID = loai_hang.LOAIID
                        AND ton_kho.SL_TON = 1
                        AND danh_muc_hang_hoa.SU_DUNG = 1
                        AND nhom_hang.SU_DUNG = 1
                    GROUP BY
                        nhom_hang.NHOM_TEN;";

                await using var cmd = new MySqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                if (reader != null)
                {
                    DanhSachTonKhoVang.Clear();

                    while (await reader.ReadAsync())
                    {
                        DanhSachTonKhoVang.Add(new TonKhoVangModel
                        {
                            NHOM_TEN = reader["NHOM_TEN"] == DBNull.Value ? string.Empty : reader.GetString("NHOM_TEN"),
                            CAN_TONG = reader["CAN_TONG"] == DBNull.Value ? 0 : reader.GetDecimal("CAN_TONG"),
                            TL_HOT = reader["TL_HOT"] == DBNull.Value ? 0 : reader.GetDecimal("TL_HOT"),
                            CONG_GOC = reader["CONG_GOC"] == DBNull.Value ? 0 : reader.GetDecimal("CONG_GOC"),
                            GIA_CONG = reader["GIA_CONG"] == DBNull.Value ? 0 : reader.GetDecimal("GIA_CONG"),
                            DON_GIA_BAN = reader["DON_GIA_BAN"] == DBNull.Value ? 0 : reader.GetDecimal("DON_GIA_BAN"),
                            SL_TON = reader["SL_TON"] == DBNull.Value ? 0 : reader.GetInt32("SL_TON")
                        });
                    }

                    DanhSachHienThi = DanhSachTonKhoVang;
                }
                else
                {
                    LoadingMessage = "Lỗi khi đọc dữ liệu từ database.";
                }
            }
            catch (Exception ex)
            {
                LoadingMessage = $"Lỗi khi tải dữ liệu: {ex.Message}";
                Console.WriteLine($"Lỗi LoadDanhSachTonKhoVang: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ThucHienTimKiem()
        {
            if (string.IsNullOrWhiteSpace(TuKhoaTimKiem))
            {
                DanhSachHienThi = DanhSachTonKhoVang;
            }
            else
            {
                string tuKhoa = TuKhoaTimKiem.ToLower();
                var ketQuaTimKiem = DanhSachTonKhoVang.Where(item =>
                    item.NHOM_TEN.ToLower().Contains(tuKhoa) ||
                    item.SL_TON.ToString().Contains(tuKhoa)
                ).ToObservableCollection();

                DanhSachHienThi = ketQuaTimKiem;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static class CollectionExtension
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source) =>
            new(source ?? Enumerable.Empty<T>());
    }
}
