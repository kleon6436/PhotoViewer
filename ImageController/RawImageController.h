/**
 * @file	RawImageController.h
 * @author	kleon6436
 */

#pragma once

#include "IImageController.h"
#include <opencv2/opencv.hpp>
#include <libraw/libraw_types.h>

namespace Kchary::ImageController::RawImageControl
{
	class RawImageController final : public IImageController
	{
	public:
		/*!
		 * @brief コンストラクタ
		 */
		RawImageController() = default;

		/*!
		* @brief デストラクタ
		*/
		~RawImageController() = default;

		/*!
		* @brief 画像を読み込み、画像サイズを取得する
		* @param path: 画像ファイルパス
		* @param imageReadSettings	画像読み込み設定
		* @param imageSize	画像サイズ(out)
		* @return 成功: True, 失敗: False
		*/
		bool LoadImageAndGetImageSize(const wchar_t* path, const ImageReadSettings& imageReadSettings, int& imageSize) override;

		/*!
		 * @brief	画像データを取得する
		 * @param	imageData: 画像データ
		 * @return	成功: True, 失敗: False
		 */
		bool GetImageData(ImageData& imageData) override;

		/*!
		 * @brief	画像のサムネイルデータを取得する
		 * @param	imageData: 画像データ
		 * @return	成功: True, 失敗: False
		 */
		bool GetThumbnailImageData(ImageData& imageData) override;

	private:
		/*!
		 * @brief    画像取得モード(OpenCV)を取得する
		 * @param   thumbnail: サムネイル画像データ
		 * @param    resizeLongSideLength: リサイズする長辺の長さ
		 * @return    ImreadModes
		 */
		static cv::ImreadModes GetImreadMode(const libraw_thumbnail_t& thumbnail, const int resizeLongSideLength);

		cv::Mat m_image;	//!< 画像
	};
}