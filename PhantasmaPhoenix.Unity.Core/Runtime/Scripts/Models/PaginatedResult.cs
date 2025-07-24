namespace Phantasma.SDK{
	public class PaginatedResult<T>
	{
		public uint Page { get; set; }
		public uint PageSize { get; set; }
		public uint Total { get; set; }
		public uint TotalPages { get; set; }
		public T? Result { get; set; }
	}
}
