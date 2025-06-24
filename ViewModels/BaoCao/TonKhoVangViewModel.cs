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

        // Thống kê tổng hợp
        private int _tongSoNhom;
        public int TongSoNhom
        {
            get => _tongSoNhom;
            set { _tongSoNhom = value; OnPropertyChanged(); }
        }
        private decimal _tongCanTong;
        public decimal TongCanTong
        {
            get => _tongCanTong;
            set { _tongCanTong = value; OnPropertyChanged(); OnPropertyChanged(nameof(TongCanTongFormatted)); }
        }
        private decimal _tongTLHot;
        public decimal TongTLHot
        {
            get => _tongTLHot;
            set { _tongTLHot = value; OnPropertyChanged(); OnPropertyChanged(nameof(TongTLHotFormatted)); }
        }
        private decimal _tongTLThuc;
        public decimal TongTLThuc
        {
            get => _tongTLThuc;
            set { _tongTLThuc = value; OnPropertyChanged(); OnPropertyChanged(nameof(TongTLThucFormatted)); }
        }
        private decimal _tongCongGoc;
        public decimal TongCongGoc
        {
            get => _tongCongGoc;
            set { _tongCongGoc = value; OnPropertyChanged(); }
        }
        private decimal _tongGiaCong;
        public decimal TongGiaCong
        {
            get => _tongGiaCong;
            set { _tongGiaCong = value; OnPropertyChanged(); }
        }
        private int _tongSoLuongTon;
        public int TongSoLuongTon
        {
            get => _tongSoLuongTon;
            set { _tongSoLuongTon = value; OnPropertyChanged(); }
        }
        private decimal _tongThanhTien;
        public decimal TongThanhTien
        {
            get => _tongThanhTien;
            set { _tongThanhTien = value; OnPropertyChanged(); }
        }
        public string TongCanTongFormatted => $"{TongCanTong:0.##} g";
        public string TongTLHotFormatted => $"{TongTLHot:0.##} g";
        public string TongTLThucFormatted => $"{TongTLThuc:0.##} g";

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
                        nhom_hang.GHI_CHU,
                        ton_kho.SL_TON,
                        nhom_hang.DON_GIA_BAN,
                        danh_muc_hang_hoa.HANGHOAID,
                        danh_muc_hang_hoa.CAN_TONG,
                        danh_muc_hang_hoa.TL_HOT,
                        danh_muc_hang_hoa.CAN_TONG - danh_muc_hang_hoa.TL_HOT AS TL_THUC,
                        danh_muc_hang_hoa.CONG_GOC,
                        danh_muc_hang_hoa.GIA_CONG
                    FROM
                        danh_muc_hang_hoa
                        JOIN ton_kho ON danh_muc_hang_hoa.HANGHOAID = ton_kho.HANGHOAID
                        JOIN nhom_hang ON danh_muc_hang_hoa.NHOMHANGID = nhom_hang.NHOMHANGID
                        JOIN loai_hang ON danh_muc_hang_hoa.LOAIID = loai_hang.LOAIID
                    WHERE
                        ton_kho.SL_TON >= 1
                        AND danh_muc_hang_hoa.SU_DUNG = 1
                        AND nhom_hang.SU_DUNG = 1
                    LIMIT 100
                ";

                await using var cmd = new MySqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                if (reader != null)
                {
                    DanhSachTonKhoVang.Clear();

                    while (await reader.ReadAsync())
                    {
                        try
                        {
                            DanhSachTonKhoVang.Add(new TonKhoVangModel
                            {
                                NHOM_TEN = reader["NHOM_TEN"] == DBNull.Value ? string.Empty : reader.GetString("NHOM_TEN"),
                                CAN_TONG = reader["CAN_TONG"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["CAN_TONG"]),
                                TL_HOT = reader["TL_HOT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TL_HOT"]),
                                CONG_GOC = reader["CONG_GOC"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["CONG_GOC"]),
                                GIA_CONG = reader["GIA_CONG"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["GIA_CONG"]),
                                DON_GIA_BAN = reader["DON_GIA_BAN"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["DON_GIA_BAN"]),
                                SL_TON = reader["SL_TON"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SL_TON"])
                            });
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Lỗi đọc dòng dữ liệu: {ex.Message}");
                        }
                    }

                    DanhSachHienThi = DanhSachTonKhoVang;

                    // Tính toán thống kê tổng hợp (sum toàn bộ, không group)
                    TongSoNhom = DanhSachTonKhoVang.Count; // Số dòng chi tiết
                    TongCanTong = DanhSachTonKhoVang.Sum(x => x.CAN_TONG);
                    TongTLHot = DanhSachTonKhoVang.Sum(x => x.TL_HOT);
                    TongTLThuc = DanhSachTonKhoVang.Sum(x => x.TL_THUC); // Sử dụng property TL_THUC của model
                    TongCongGoc = DanhSachTonKhoVang.Sum(x => x.CONG_GOC);
                    TongGiaCong = DanhSachTonKhoVang.Sum(x => x.GIA_CONG);
                    TongSoLuongTon = DanhSachTonKhoVang.Sum(x => x.SL_TON);
                    TongThanhTien = DanhSachTonKhoVang.Sum(x => x.ThanhTien);
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
