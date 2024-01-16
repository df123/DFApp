export interface ChartJSDatasetsItemDto {
	label: string;
	data: number[];
}

export interface ChartJSDto {
	labels: string[];
	datasets: ChartJSDatasetsItemDto[];
	total: number,
	compareTotal: number,
	differenceTotal: number
}