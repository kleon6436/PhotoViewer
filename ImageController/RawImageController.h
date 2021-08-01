/**
 * @file	RawImageController.h
 * @author	kleon6436
 */

#pragma once

#include <opencv2/opencv.hpp>
#include <libraw/libraw_types.h>
#include "IImageController.h"

namespace Kchary::ImageController::RawImageControl
{
	class RawImageController final : public IImageController
	{
	public:
		/**
		 * @brief	LibRawライブラリを用いて画像データを取得する
		 * @param	path: 画像ファイルパス
		 * @param	imageData: 画像データ
		 * @return	成功: 0, 失敗: -1
		 */
		int GetImageData(const char path[], ImageData& imageData) const override;

		/**
		 * @brief	LibRawライブラリを用いて画像サムネイルデータを取得する
		 * @param	path: 画像ファイルパス
		 * @param	resizeLongSideLength: リサイズする長辺の長さ
		 * @param	imageData: 画像データ
		 * @return	成功: 0, 失敗: -1
		 */
		int GetThumbnailImageData(const char path[], int resizeLongSideLength, ImageData& imageData) const override;

	private:
		/**
		 * @brief	画像取得モード(OpenCV)を取得する
		 * @param   thumbnail: サムネイル画像データ
		 * @param	resizeLongSideLength: リサイズする長辺の長さ
		 * @return	ImreadModes
		 */
		static cv::ImreadModes GetImreadMode(libraw_thumbnail_t thumbnail, int resizeLongSideLength);
	};
}