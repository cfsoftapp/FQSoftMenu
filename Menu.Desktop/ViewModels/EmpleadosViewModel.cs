using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Menu.DTOs.Empleados;
using Menu.Enums;
using Menu.Models;
using Menu.Services;
using Microsoft.Win32;

namespace Menu.Desktop.ViewModels;

public sealed class EmpleadosViewModel : ObservableObject
{
    private readonly EmpleadoService _empleadoService;
    private readonly TipoEmpleadoService _tipoEmpleadoService;
    private readonly EmpresaClienteService _empresaClienteService;
    private readonly SucursalService _sucursalService;
    private readonly AsyncRelayCommand _refreshCommand;
    private readonly AsyncRelayCommand _saveCommand;
    private readonly RelayCommand _previousPageCommand;
    private readonly RelayCommand _nextPageCommand;
    private readonly RelayCommand _newCommand;
    private readonly RelayCommand _cancelCommand;
    private readonly RelayCommand<EmpleadoRowViewModel> _editCommand;
    private readonly RelayCommand<EmpleadoRowViewModel> _toggleEstadoCommand;
    private readonly RelayCommand<EmpleadoRowViewModel> _toggleActivoCommand;
    private readonly AsyncRelayCommand _importCsvCommand;
    private readonly RelayCommand _exportFormatCommand;
    private readonly List<EmpleadoRowViewModel> _allRows = new();
    private readonly List<EmpleadoRowViewModel> _filteredRows = new();
    private string _searchText = string.Empty;
    private string _estado = "Listo para cargar comensales.";
    private bool _isBusy;
    private bool _showForm;
    private int _editingId;
    private string _formTitle = "Nuevo comensal";
    private string _formDni = string.Empty;
    private string _formNombres = string.Empty;
    private string _formApellidos = string.Empty;
    private int? _formTipoEmpleadoId;
    private int? _formEmpresaClienteId;
    private int? _formSucursalId;
    private EstadoEmpleado _formEstado = EstadoEmpleado.Activo;
    private CategoriaEmpleado _formCategoria = CategoriaEmpleado.Obrero;
    private bool _formActivo = true;
    private int _currentPage = 1;
    private int _pageSize = 5;

    public EmpleadosViewModel(
        EmpleadoService empleadoService,
        TipoEmpleadoService tipoEmpleadoService,
        EmpresaClienteService empresaClienteService,
        SucursalService sucursalService)
    {
        _empleadoService = empleadoService;
        _tipoEmpleadoService = tipoEmpleadoService;
        _empresaClienteService = empresaClienteService;
        _sucursalService = sucursalService;
        _refreshCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
        _saveCommand = new AsyncRelayCommand(SaveAsync, () => !IsBusy && ShowForm);
        _previousPageCommand = new RelayCommand(PreviousPage, () => CanPreviousPage);
        _nextPageCommand = new RelayCommand(NextPage, () => CanNextPage);
        _newCommand = new RelayCommand(NewEmpleado, () => !IsBusy);
        _cancelCommand = new RelayCommand(CancelForm);
        _editCommand = new RelayCommand<EmpleadoRowViewModel>(EditEmpleado, row => row is not null && !IsBusy);
        _toggleEstadoCommand = new RelayCommand<EmpleadoRowViewModel>(async row => await ToggleEstadoAsync(row), row => row is not null && !IsBusy);
        _toggleActivoCommand = new RelayCommand<EmpleadoRowViewModel>(async row => await ToggleActivoAsync(row), row => row is not null && !IsBusy);
        _importCsvCommand = new AsyncRelayCommand(ImportCsvAsync, () => !IsBusy);
        _exportFormatCommand = new RelayCommand(ExportCsvFormat, () => !IsBusy);

        EstadoOptions.Add(new OptionViewModel<EstadoEmpleado>(EstadoEmpleado.Activo, "Activo"));
        EstadoOptions.Add(new OptionViewModel<EstadoEmpleado>(EstadoEmpleado.Suspendido, "Suspendido"));
    }

    public ObservableCollection<EmpleadoRowViewModel> Empleados { get; } = new();

    public ObservableCollection<OptionViewModel<int?>> TipoEmpleadoOptions { get; } = new();

    public ObservableCollection<OptionViewModel<int?>> EmpresaClienteOptions { get; } = new();

    public ObservableCollection<OptionViewModel<int?>> SucursalOptions { get; } = new();

    public ObservableCollection<OptionViewModel<EstadoEmpleado>> EstadoOptions { get; } = new();

    public ObservableCollection<int> PageSizeOptions { get; } = new(new[] { 5, 10, 20, 50 });

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (SetProperty(ref _pageSize, value))
                ApplyFilter(resetPage: true);
        }
    }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(_filteredRows.Count / (double)PageSize));

    public string PageInfo => _filteredRows.Count == 0
        ? "Sin registros"
        : $"{GetFirstVisibleIndex()}-{GetLastVisibleIndex()} de {_filteredRows.Count}";

    public string CurrentPageText => $"Pagina {_currentPage} de {TotalPages}";

    public bool CanPreviousPage => _currentPage > 1;

    public bool CanNextPage => _currentPage < TotalPages;

    public event Action? RequestFormDialog;

    public bool ShowForm
    {
        get => _showForm;
        private set
        {
            if (SetProperty(ref _showForm, value))
                _saveCommand.RaiseCanExecuteChanged();
        }
    }

    public string FormTitle
    {
        get => _formTitle;
        private set => SetProperty(ref _formTitle, value);
    }

    public string FormDni
    {
        get => _formDni;
        set => SetProperty(ref _formDni, value);
    }

    public bool IsEditing => _editingId != 0;

    public string FormNombres
    {
        get => _formNombres;
        set => SetProperty(ref _formNombres, value);
    }

    public string FormApellidos
    {
        get => _formApellidos;
        set => SetProperty(ref _formApellidos, value);
    }

    public int? FormTipoEmpleadoId
    {
        get => _formTipoEmpleadoId;
        set => SetProperty(ref _formTipoEmpleadoId, value);
    }

    public int? FormEmpresaClienteId
    {
        get => _formEmpresaClienteId;
        set
        {
            if (SetProperty(ref _formEmpresaClienteId, value))
            {
                LoadSucursalOptions();
                if (FormSucursalId.HasValue && !SucursalOptions.Any(x => x.Value == FormSucursalId))
                    FormSucursalId = SucursalOptions.FirstOrDefault()?.Value;
            }
        }
    }

    public int? FormSucursalId
    {
        get => _formSucursalId;
        set => SetProperty(ref _formSucursalId, value);
    }

    public EstadoEmpleado FormEstado
    {
        get => _formEstado;
        set => SetProperty(ref _formEstado, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ApplyFilter(resetPage: true);
        }
    }

    public string Estado
    {
        get => _estado;
        private set => SetProperty(ref _estado, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                _refreshCommand.RaiseCanExecuteChanged();
                _saveCommand.RaiseCanExecuteChanged();
                _newCommand.RaiseCanExecuteChanged();
                _editCommand.RaiseCanExecuteChanged();
                _toggleEstadoCommand.RaiseCanExecuteChanged();
                _toggleActivoCommand.RaiseCanExecuteChanged();
                _importCsvCommand.RaiseCanExecuteChanged();
                _exportFormatCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand RefreshCommand => _refreshCommand;

    public ICommand NewCommand => _newCommand;

    public ICommand SaveCommand => _saveCommand;

    public ICommand CancelCommand => _cancelCommand;

    public ICommand EditCommand => _editCommand;

    public ICommand ToggleEstadoCommand => _toggleEstadoCommand;

    public ICommand ToggleActivoCommand => _toggleActivoCommand;

    public ICommand ImportCsvCommand => _importCsvCommand;

    public ICommand ExportFormatCommand => _exportFormatCommand;

    public ICommand PreviousPageCommand => _previousPageCommand;

    public ICommand NextPageCommand => _nextPageCommand;

    public async Task LoadAsync()
    {
        IsBusy = true;
        Estado = "Cargando comensales...";

        try
        {
            await LoadCatalogsAsync();
            var empleados = await _empleadoService.GetAllAsync();

            _allRows.Clear();
            _allRows.AddRange(empleados.Select(x => new EmpleadoRowViewModel(x)));

            ApplyFilter(resetPage: true);
        }
        catch (Exception ex)
        {
            Estado = $"No se pudo cargar comensales: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadCatalogsAsync()
    {
        if (TipoEmpleadoOptions.Count == 0)
        {
            var tipos = await _tipoEmpleadoService.GetActivosAsync();
            TipoEmpleadoOptions.Clear();
            foreach (var tipo in tipos)
            {
                TipoEmpleadoOptions.Add(new OptionViewModel<int?>(tipo.Id, tipo.Nombre));
            }
        }

        if (EmpresaClienteOptions.Count == 0)
        {
            var empresas = await _empresaClienteService.GetActivasAsync();
            EmpresaClienteOptions.Clear();
            foreach (var empresa in empresas)
            {
                EmpresaClienteOptions.Add(new OptionViewModel<int?>(empresa.Id, empresa.NombreComercial));
            }
        }

        LoadSucursalOptions(await _sucursalService.GetAllAsync());
    }

    private List<Sucursal> _sucursales = new();

    private void LoadSucursalOptions(List<Sucursal>? sucursales = null)
    {
        if (sucursales is not null)
            _sucursales = sucursales;

        SucursalOptions.Clear();

        foreach (var sucursal in _sucursales
                     .Where(x => x.Activo)
                     .Where(x => !FormEmpresaClienteId.HasValue ||
                                 !x.EmpresaClienteId.HasValue ||
                                 x.EmpresaClienteId == FormEmpresaClienteId)
                     .OrderBy(x => x.Nombre))
        {
            SucursalOptions.Add(new OptionViewModel<int?>(sucursal.Id, sucursal.Nombre));
        }
    }

    private void NewEmpleado()
    {
        _editingId = 0;
        FormTitle = "Nuevo comensal";
        FormDni = string.Empty;
        FormNombres = string.Empty;
        FormApellidos = string.Empty;
        FormEstado = EstadoEmpleado.Activo;
        _formCategoria = CategoriaEmpleado.Obrero;
        _formActivo = true;
        FormTipoEmpleadoId = TipoEmpleadoOptions.FirstOrDefault(x => x.Text == "Obrero")?.Value ??
                             TipoEmpleadoOptions.FirstOrDefault()?.Value;
        FormEmpresaClienteId = EmpresaClienteOptions.FirstOrDefault()?.Value;
        FormSucursalId = SucursalOptions.FirstOrDefault()?.Value;
        ShowForm = true;
        OnPropertyChanged(nameof(IsEditing));
        RequestFormDialog?.Invoke();
    }

    private void EditEmpleado(EmpleadoRowViewModel? row)
    {
        if (row is null)
            return;

        var empleado = row.Empleado;
        _editingId = empleado.Id;
        FormTitle = "Editar comensal";
        FormDni = empleado.Dni;
        FormNombres = empleado.Nombres;
        FormApellidos = empleado.Apellidos;
        FormEstado = empleado.Estado;
        _formCategoria = empleado.Categoria;
        _formActivo = empleado.Activo;
        FormTipoEmpleadoId = empleado.TipoEmpleadoId;
        FormEmpresaClienteId = empleado.EmpresaClienteId;
        FormSucursalId = empleado.SucursalId;
        ShowForm = true;
        OnPropertyChanged(nameof(IsEditing));
        RequestFormDialog?.Invoke();
    }

    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FormDni) ||
            string.IsNullOrWhiteSpace(FormNombres) ||
            string.IsNullOrWhiteSpace(FormApellidos))
        {
            Estado = "Complete DNI, nombres y apellidos.";
            return;
        }

        IsBusy = true;

        try
        {
            var empleado = new Empleado
            {
                Id = _editingId,
                Dni = FormDni,
                Nombres = FormNombres,
                Apellidos = FormApellidos,
                Estado = FormEstado,
                Categoria = _formCategoria,
                TipoEmpleadoId = FormTipoEmpleadoId,
                EmpresaClienteId = FormEmpresaClienteId,
                SucursalId = FormSucursalId,
                Activo = _formActivo
            };

            var result = _editingId == 0
                ? await _empleadoService.CreateAsync(empleado)
                : await _empleadoService.UpdateAsync(empleado);

            Estado = result.Message;

            if (result.Success)
            {
                ShowForm = false;
                await ReloadRowsAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void CancelForm()
    {
        ShowForm = false;
        _editingId = 0;
        OnPropertyChanged(nameof(IsEditing));
    }

    private async Task ToggleEstadoAsync(EmpleadoRowViewModel? row)
    {
        if (row is null)
            return;

        IsBusy = true;

        try
        {
            var result = await _empleadoService.ToggleEstadoBeneficioAsync(row.Id);
            Estado = result.Message;
            await ReloadRowsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ToggleActivoAsync(EmpleadoRowViewModel? row)
    {
        if (row is null)
            return;

        IsBusy = true;

        try
        {
            var result = await _empleadoService.ToggleActivoAsync(row.Id);
            Estado = result.Message;
            await ReloadRowsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReloadRowsAsync()
    {
        var empleados = await _empleadoService.GetAllAsync();
        _allRows.Clear();
        _allRows.AddRange(empleados.Select(x => new EmpleadoRowViewModel(x)));
        ApplyFilter(resetPage: false);
    }

    private void ExportCsvFormat()
    {
        var dialog = new SaveFileDialog
        {
            FileName = "formato-carga-comensales.csv",
            Filter = "CSV (*.csv)|*.csv",
            OverwritePrompt = true
        };

        if (dialog.ShowDialog() != true)
            return;

        var contenido = new StringBuilder();
        contenido.AppendLine("DNI,Nombres,Apellidos,TipoComensal,Estado,Activo");
        contenido.AppendLine("12345678,Juan,Carrasco,Obrero,Activo,Si");
        File.WriteAllText(dialog.FileName, contenido.ToString(), Encoding.UTF8);
        Estado = "Formato de carga generado correctamente.";
    }

    private async Task ImportCsvAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "CSV (*.csv)|*.csv|Todos los archivos (*.*)|*.*",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
            return;

        IsBusy = true;

        try
        {
            var contenido = await File.ReadAllTextAsync(dialog.FileName, Encoding.UTF8);
            var filas = ParsearCsvEmpleados(contenido);
            var previsualizacion = await _empleadoService.PrevisualizarCargaMasivaAsync(filas);

            if (previsualizacion.Pendientes == 0)
            {
                Estado = $"Carga revisada. Observados: {previsualizacion.Observados}. No hay registros validos para importar.";
                MessageBox.Show(Estado, "Carga masiva", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"Listos para importar: {previsualizacion.Pendientes} de {previsualizacion.TotalFilas}.\nObservados: {previsualizacion.Observados}.\n\nDesea confirmar la carga?",
                "Confirmar carga masiva",
                MessageBoxButton.YesNo,
                previsualizacion.Observados > 0 ? MessageBoxImage.Warning : MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes)
            {
                Estado = $"Carga revisada sin importar. Listos: {previsualizacion.Pendientes}. Observados: {previsualizacion.Observados}.";
                return;
            }

            var resultado = await _empleadoService.ImportarCargaMasivaAsync(filas);
            Estado = $"Carga confirmada. Importados: {resultado.Importados}. Observados: {resultado.Observados}.";
            await ReloadRowsAsync();
        }
        catch (Exception ex)
        {
            Estado = $"Error en carga masiva: {ex.Message}";
            MessageBox.Show(Estado, "Carga masiva", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter(bool resetPage)
    {
        var text = SearchText.Trim().ToLowerInvariant();
        var rows = string.IsNullOrWhiteSpace(text)
            ? _allRows
            : _allRows
                .Where(x =>
                    x.Dni.ToLowerInvariant().Contains(text) ||
                    x.NombreCompleto.ToLowerInvariant().Contains(text) ||
                    x.EmpresaCliente.ToLowerInvariant().Contains(text) ||
                    x.Sucursal.ToLowerInvariant().Contains(text) ||
                    x.Categoria.ToLowerInvariant().Contains(text) ||
                    x.Estado.ToLowerInvariant().Contains(text))
                .ToList();

        _filteredRows.Clear();
        _filteredRows.AddRange(rows);

        if (resetPage)
            _currentPage = 1;

        if (_currentPage > TotalPages)
            _currentPage = TotalPages;

        ApplyPage();
    }

    private void ApplyPage()
    {
        Empleados.Clear();
        foreach (var row in _filteredRows.Skip((_currentPage - 1) * PageSize).Take(PageSize))
        {
            Empleados.Add(row);
        }

        Estado = $"{_filteredRows.Count} comensales visibles de {_allRows.Count}.";
        NotifyPaginationChanged();
    }

    private void PreviousPage()
    {
        if (!CanPreviousPage)
            return;

        _currentPage--;
        ApplyPage();
    }

    private void NextPage()
    {
        if (!CanNextPage)
            return;

        _currentPage++;
        ApplyPage();
    }

    private int GetFirstVisibleIndex()
    {
        return _filteredRows.Count == 0 ? 0 : ((_currentPage - 1) * PageSize) + 1;
    }

    private int GetLastVisibleIndex()
    {
        return Math.Min(_currentPage * PageSize, _filteredRows.Count);
    }

    private void NotifyPaginationChanged()
    {
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(PageInfo));
        OnPropertyChanged(nameof(CurrentPageText));
        OnPropertyChanged(nameof(CanPreviousPage));
        OnPropertyChanged(nameof(CanNextPage));
        OnPropertyChanged(nameof(PageSize));
        _previousPageCommand.RaiseCanExecuteChanged();
        _nextPageCommand.RaiseCanExecuteChanged();
        _newCommand.RaiseCanExecuteChanged();
        _editCommand.RaiseCanExecuteChanged();
        _toggleEstadoCommand.RaiseCanExecuteChanged();
        _toggleActivoCommand.RaiseCanExecuteChanged();
        _saveCommand.RaiseCanExecuteChanged();
    }

    private static List<EmpleadoCargaMasivaFilaDto> ParsearCsvEmpleados(string contenido)
    {
        var filas = new List<EmpleadoCargaMasivaFilaDto>();
        var lineas = contenido
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        for (var index = 1; index < lineas.Length; index++)
        {
            var columnas = ParsearLineaCsv(lineas[index]);

            if (columnas.Count == 0 || columnas.All(string.IsNullOrWhiteSpace))
                continue;

            filas.Add(new EmpleadoCargaMasivaFilaDto
            {
                NumeroFila = index + 1,
                Dni = GetColumna(columnas, 0),
                Nombres = GetColumna(columnas, 1),
                Apellidos = GetColumna(columnas, 2),
                TipoPersonalTexto = GetColumna(columnas, 3),
                EstadoTexto = GetColumna(columnas, 4),
                ActivoTexto = GetColumna(columnas, 5)
            });
        }

        return filas;
    }

    private static List<string> ParsearLineaCsv(string linea)
    {
        var columnas = new List<string>();
        var actual = new StringBuilder();
        var enComillas = false;

        for (var i = 0; i < linea.Length; i++)
        {
            var caracter = linea[i];

            if (caracter == '"')
            {
                if (enComillas && i + 1 < linea.Length && linea[i + 1] == '"')
                {
                    actual.Append('"');
                    i++;
                }
                else
                {
                    enComillas = !enComillas;
                }

                continue;
            }

            if (caracter == ',' && !enComillas)
            {
                columnas.Add(actual.ToString().Trim());
                actual.Clear();
                continue;
            }

            actual.Append(caracter);
        }

        columnas.Add(actual.ToString().Trim());
        return columnas;
    }

    private static string GetColumna(List<string> columnas, int index)
    {
        return index < columnas.Count ? columnas[index] : string.Empty;
    }
}
