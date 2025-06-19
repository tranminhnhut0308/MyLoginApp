using System.ComponentModel;

public class LoaiVangModel : INotifyPropertyChanged
{
    private bool _isSelected;
    private string _tenLoaiVang;
    private decimal? _donGiaVon;
    private decimal? _donGiaMua;
    private decimal? _donGiaBan;
    private decimal? _donGiaCam;
    private int _nhomHangID;
    private string _nhomHangMA;
    private int? _nhomChaID;
    private string _muaBan;
    private int _suDung;
    private string _ghiChu;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }

    public int NhomHangID
    {
        get => _nhomHangID;
        set
        {
            if (_nhomHangID != value)
            {
                _nhomHangID = value;
                OnPropertyChanged(nameof(NhomHangID));
            }
        }
    }

    public string NhomHangMA
    {
        get => _nhomHangMA;
        set
        {
            if (_nhomHangMA != value)
            {
                _nhomHangMA = value;
                OnPropertyChanged(nameof(NhomHangMA));
            }
        }
    }

    public int? NhomChaID
    {
        get => _nhomChaID;
        set
        {
            if (_nhomChaID != value)
            {
                _nhomChaID = value;
                OnPropertyChanged(nameof(NhomChaID));
            }
        }
    }

    public string TenLoaiVang
    {
        get => _tenLoaiVang;
        set
        {
            if (_tenLoaiVang != value)
            {
                _tenLoaiVang = value;
                OnPropertyChanged(nameof(TenLoaiVang));
            }
        }
    }

    public decimal? DonGiaVon
    {
        get => _donGiaVon;
        set
        {
            if (_donGiaVon != value)
            {
                _donGiaVon = value;
                OnPropertyChanged(nameof(DonGiaVon));
            }
        }
    }

    public decimal? DonGiaMua
    {
        get => _donGiaMua;
        set
        {
            if (_donGiaMua != value)
            {
                _donGiaMua = value;
                OnPropertyChanged(nameof(DonGiaMua));
            }
        }
    }

    public decimal? DonGiaBan
    {
        get => _donGiaBan;
        set
        {
            if (_donGiaBan != value)
            {
                _donGiaBan = value;
                OnPropertyChanged(nameof(DonGiaBan));
            }
        }
    }

    public decimal? DonGiaCam
    {
        get => _donGiaCam;
        set
        {
            if (_donGiaCam != value)
            {
                _donGiaCam = value;
                OnPropertyChanged(nameof(DonGiaCam));
            }
        }
    }

    public string MuaBan
    {
        get => _muaBan;
        set
        {
            if (_muaBan != value)
            {
                _muaBan = value;
                OnPropertyChanged(nameof(MuaBan));
            }
        }
    }

    public int SuDung
    {
        get => _suDung;
        set
        {
            if (_suDung != value)
            {
                _suDung = value;
                OnPropertyChanged(nameof(SuDung));
            }
        }
    }

    public string GhiChu
    {
        get => _ghiChu;
        set
        {
            if (_ghiChu != value)
            {
                _ghiChu = value;
                OnPropertyChanged(nameof(GhiChu));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    
    protected void OnPropertyChanged(string propertyName) => 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
