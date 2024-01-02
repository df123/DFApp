import { LotteryStructure } from "./LotteryStructure";

export interface StatisticsWinItemDto {
	code: string;
	winCode: string;
	winAmount: number;
	buyLottery: LotteryStructure[];
	buyLotteryString: string;
	winLottery: LotteryStructure[];
	winLotteryString: string;
}