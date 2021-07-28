/**
 * @file	NormalImageController.h
 * @author	kleon6436
 */

#pragma once

#include <opencv2/opencv.hpp>
#include "IImageController.h"

namespace Kchary::ImageController::NormalImageControl
{
	class NormalImageController final : public IImageController
	{
	public:
		/**
		 * @brief	画像データを取得する
		 * @param	path: 画像ファイルパス
		 * @param	imageData: 画像データ
		 * @return	成功: 0, 失敗: -1
		 */
		int GetImageData(const char path[], ImageData& imageData) const override;

		/**
		 * @brief	画像のサムネイルデータを取得する
		 * @param	path: ファイルパス
		 * @param	resizeLongSideLength: リサイズする長辺の長さ
		 * @param	imageData: 画像データ
		 * @return	成功: 0, 失敗: -1
		 */
		int GetThumbnailImageData(const char path[], int resizeLongSideLength, ImageData& imageData) const override;

	private:
		/**
		 * @brief	画像取得モード(OpenCV)を取得する
		 * @param	resizeLongSideLength: リサイズする長辺の長さ
		 * @return	ImreadModes
		 */
		static cv::ImreadModes GetImreadMode(int resizeLongSideLength);
	};
}