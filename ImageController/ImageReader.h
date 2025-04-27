/*!
 * @file	ImageReader.h
 * @author	kleon6436
 */

#pragma once

#include "ImageData.h"
#include "IImageController.h"
#include <memory>

namespace Kchary::ImageController::Library
{
	class ImageReader final
	{
	public:
		/*!
		* @brief コンストラクタ
		*/
		ImageReader();

		/*!
		* @brief デストラクタ
		*/
		~ImageReader() = default;

		/*!
		 * @brief	画像データを取得する
		 * @param	imageData: 画像データ
		 * @return	成功: True, 失敗: False
		 */
		bool GetImageData(const wchar_t* imagePath, const ImageReadSettings& imageReadSettings, ImageData& imageData);

	private:
		std::unique_ptr<IImageController> m_rawImageController;		//!< RAW画像読み込み用インスタンス
		std::unique_ptr<IImageController> m_normalImageController;	//!< 通常の画像読み込み用インスタンス
	};
}