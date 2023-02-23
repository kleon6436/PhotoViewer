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
		 * @brief	画像データを取得する
		 * @param	path							画像パス
		 * @param	imageReadSettings	画像設定
		 * @param	imageData				画像データ(out)
		 * @return	成功: True, 失敗: False
		 */
		bool GetImageData(const wchar_t* path, const ImageReadSettings& imageReadSettings, ImageData& imageData) override;

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